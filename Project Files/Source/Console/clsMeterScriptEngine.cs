/*  clsMeterScriptEngine.cs

This file is part of a program that implements a Software-Defined Radio.

This code/file can be found on GitHub : https://github.com/ramdor/Thetis

Copyright (C) 2020-2025 Richard Samphire MW0LGE

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

The author can be reached by email at

mw0lge@grange-lane.co.uk
*/
//
//============================================================================================//
// Dual-Licensing Statement (Applies Only to Author's Contributions, Richard Samphire MW0LGE) //
// ------------------------------------------------------------------------------------------ //
// For any code originally written by Richard Samphire MW0LGE, or for any modifications       //
// made by him, the copyright holder for those portions (Richard Samphire) reserves the       //
// right to use, license, and distribute such code under different terms, including           //
// closed-source and proprietary licences, in addition to the GNU General Public License      //
// granted above. Nothing in this statement restricts any rights granted to recipients under  //
// the GNU GPL. Code contributed by others (not Richard Samphire) remains licensed under      //
// its original terms and is not affected by this dual-licensing statement in any way.        //
// Richard Samphire can be reached by email at :  mw0lge@grange-lane.co.uk                    //
//============================================================================================//


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Thetis
{
    public static class MeterScriptEngine
    {
        public sealed class BankVars
        {
            private readonly Dictionary<string, object> _bank;
            private readonly Dictionary<string, object> _common;
            public BankVars(Dictionary<string, object> bank, Dictionary<string, object> common) { _bank = bank; _common = common; }
            public dynamic this[string k]
            {
                get
                {
                    if (_bank != null && _bank.ContainsKey(k)) return _bank[k];
                    if (_common != null && _common.ContainsKey(k)) return _common[k];
                    throw new KeyNotFoundException("Variable '" + k + "' not found");
                }
            }
        }

        public sealed class Snapshot
        {
            public Dictionary<string, object> Common;
            public Dictionary<string, object>[] Banks;
        }

        public class Globals
        {
            public BankVars[] Variables;
        }

        private static readonly object _lock = new object();
        private static List<string> _conditions = new List<string>();
        private static List<bool> _occupied = new List<bool>();
        private static List<int> _update_intervals_ms = new List<int>();
        private static List<long> _next_due_ticks = new List<long>();
        private static List<bool> _results = new List<bool>();
        private static List<bool> _errors = new List<bool>();
        private static List<string> _diagnostics = new List<string>();
        private static Queue<int> _free_indices = new Queue<int>();
        private static ScriptRunner<bool[]> _runner = null;
        private static bool _needs_recompile = false;
        private static Timer _timer = null;
        private static int _default_interval_ms = 100;
        private static int _loop_interval_ms = 100;
        private static int _bank_count = 1;
        private static Func<Snapshot> _variable_provider_banked = null;
        private static readonly long _ticks_per_millisecond = TimeSpan.TicksPerMillisecond;
        private static int _batch_depth = 0;
        private static bool _loop_interval_dirty = false;

        public static void start(Func<Snapshot> variable_provider_banked, int default_interval_ms, int bank_count)
        {
            lock (_lock)
            {
                _variable_provider_banked = variable_provider_banked;
                _bank_count = bank_count < 1 ? 1 : bank_count;
                _default_interval_ms = default_interval_ms < 1 ? 1 : default_interval_ms;
                _loop_interval_ms = _default_interval_ms;
                if (_timer == null)
                {
                    _timer = new Timer(_ => tick(), null, _loop_interval_ms, _loop_interval_ms);
                }
                else
                {
                    _timer.Change(_loop_interval_ms, _loop_interval_ms);
                }
            }
        }

        public static void stop()
        {
            lock (_lock)
            {
                if (_timer != null)
                {
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    _timer.Dispose();
                    _timer = null;
                }
                _runner = null;
            }
        }

        public static void BeginBatch()
        {
            lock (_lock)
            {
                _batch_depth++;
            }
        }

        public static void EndBatch()
        {
            bool do_compile = false;
            bool do_recompute = false;

            lock (_lock)
            {
                if (_batch_depth > 0) _batch_depth--;
                if (_batch_depth == 0)
                {
                    if (_needs_recompile) do_compile = true;
                    if (_loop_interval_dirty) do_recompute = true;
                    _loop_interval_dirty = false;
                }
            }

            if (do_compile)
            {
                ScriptRunner<bool[]> built = build_runner();
                lock (_lock)
                {
                    _runner = built;
                    _needs_recompile = false;
                }
            }

            if (do_recompute)
            {
                lock (_lock)
                {
                    recompute_loop_interval_nolock();
                }
            }
        }

        public static int register_led()
        {
            lock (_lock)
            {
                int index;
                if (_free_indices.Count > 0)
                {
                    index = _free_indices.Dequeue();
                    _occupied[index] = true;
                    _conditions[index] = string.Empty;
                    _update_intervals_ms[index] = _default_interval_ms;
                    _next_due_ticks[index] = 0L;
                    _results[index] = false;
                    _errors[index] = false;
                    _diagnostics[index] = string.Empty;
                }
                else
                {
                    index = _conditions.Count;
                    _conditions.Add(string.Empty);
                    _occupied.Add(true);
                    _update_intervals_ms.Add(_default_interval_ms);
                    _next_due_ticks.Add(0L);
                    _results.Add(false);
                    _errors.Add(false);
                    _diagnostics.Add(string.Empty);
                }
                _needs_recompile = true;
                _loop_interval_dirty = true;
                if (_batch_depth == 0) recompute_loop_interval_nolock();
                return index;
            }
        }

        public static void unregister_led(int index)
        {
            lock (_lock)
            {
                if (index < 0 || index >= _occupied.Count) return;
                if (!_occupied[index]) return;
                _occupied[index] = false;
                _conditions[index] = string.Empty;
                _results[index] = false;
                _errors[index] = false;
                _diagnostics[index] = string.Empty;
                _update_intervals_ms[index] = _default_interval_ms;
                _next_due_ticks[index] = 0L;
                _free_indices.Enqueue(index);
                _needs_recompile = true;
                _loop_interval_dirty = true;
                if (_batch_depth == 0) recompute_loop_interval_nolock();
            }
        }

        public static bool set_condition(int index, string condition)
        {
            if (index < 0 || index >= _occupied.Count) return false;
            if (!_occupied[index]) return false;

            string expr = condition ?? string.Empty;

            bool fast_path = false;
            lock (_lock)
            {
                if (_batch_depth > 0)
                {
                    _conditions[index] = expr;
                    _diagnostics[index] = string.Empty;
                    _errors[index] = false;
                    _needs_recompile = true;
                    fast_path = true;
                }
            }
            if (fast_path) return true;

            string code = "return (bool)(" + expr + ");";

            ScriptOptions options = ScriptOptions.Default
                .AddReferences(typeof(object).Assembly)
                .AddReferences(typeof(Dictionary<string, object>).Assembly)
                .AddReferences(typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly)
                .AddReferences(typeof(Globals).Assembly)
                .AddImports("System")
                .AddImports("System.Collections.Generic")
                .AddImports("Microsoft.CSharp");

            Script<bool> script = CSharpScript.Create<bool>(code, options, typeof(Globals));
            System.Collections.Immutable.ImmutableArray<Diagnostic> diags = script.Compile();

            bool has_errors = false;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (Diagnostic d in diags)
            {
                if (d.Severity == DiagnosticSeverity.Error)
                {
                    has_errors = true;
                    sb.AppendLine(d.ToString());
                }
            }
            if (has_errors)
            {
                lock (_lock)
                {
                    _diagnostics[index] = sb.ToString();
                    _errors[index] = true;
                }
                return false;
            }

            Globals g = build_globals_once();
            try
            {
                ScriptState<bool> state = script.RunAsync(g).GetAwaiter().GetResult();
                bool tmp = state.ReturnValue;
            }
            catch (Exception ex)
            {
                lock (_lock)
                {
                    _diagnostics[index] = ex.Message;
                    _errors[index] = true;
                }
                return false;
            }

            lock (_lock)
            {
                _conditions[index] = expr;
                _diagnostics[index] = string.Empty;
                _errors[index] = false;
                _needs_recompile = true;
            }

            return true;
        }


        public static void set_update_interval(int index, int milliseconds)
        {
            lock (_lock)
            {
                if (index < 0 || index >= _occupied.Count) return;
                if (!_occupied[index]) return;
                int ms = milliseconds < 1 ? 1 : milliseconds;
                _update_intervals_ms[index] = ms;
                _loop_interval_dirty = true;
                if (_batch_depth == 0) recompute_loop_interval_nolock();
            }
        }

        public static bool read_result(int index)
        {
            lock (_lock)
            {
                if (index < 0 || index >= _occupied.Count) return false;
                if (!_occupied[index]) return false;
                return _results[index];
            }
        }

        public static bool read_error(int index)
        {
            lock (_lock)
            {
                if (index < 0 || index >= _occupied.Count) return true;
                if (!_occupied[index]) return true;
                return _errors[index];
            }
        }

        public static string read_diagnostic(int index)
        {
            lock (_lock)
            {
                if (index < 0 || index >= _occupied.Count) return string.Empty;
                if (!_occupied[index]) return string.Empty;
                return _diagnostics[index] ?? string.Empty;
            }
        }

        public static void set_loop_interval_floor(int milliseconds)
        {
            lock (_lock)
            {
                int ms = milliseconds < 1 ? 1 : milliseconds;
                if (ms > _loop_interval_ms) _loop_interval_ms = ms;
                if (_timer != null) _timer.Change(_loop_interval_ms, _loop_interval_ms);
            }
        }

        private static void tick()
        {
            Globals globals = null;
            ScriptRunner<bool[]> runner_local = null;
            List<int> due_indices = null;
            bool compile_now = false;

            lock (_lock)
            {
                if (_needs_recompile && _batch_depth == 0)
                {
                    compile_now = true;
                    _needs_recompile = false;
                }
                runner_local = _runner;
                due_indices = get_due_indices_nolock();
            }

            if (compile_now)
            {
                ScriptRunner<bool[]> built = build_runner();
                lock (_lock)
                {
                    _runner = built;
                    runner_local = _runner;
                }
            }

            if (runner_local == null) return;

            globals = build_globals_once();

            bool[] eval_results = null;
            try
            {
                eval_results = runner_local(globals).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                lock (_lock)
                {
                    int countErr = _occupied.Count;
                    for (int i = 0; i < countErr; i++)
                    {
                        if (_occupied[i])
                        {
                            _errors[i] = true;
                            _diagnostics[i] = ex.Message;
                        }
                    }
                }
                return;
            }

            if (eval_results == null) return;

            long now_ticks = DateTime.UtcNow.Ticks;

            lock (_lock)
            {
                int count = Math.Min(eval_results.Length, _results.Count);
                for (int i = 0; i < count; i++)
                {
                    if (!_occupied[i]) continue;
                    _results[i] = eval_results[i];
                }
                for (int i = 0; i < due_indices.Count; i++)
                {
                    int idx = due_indices[i];
                    if (!_occupied[idx]) continue;
                    long delay = (long)_update_intervals_ms[idx] * _ticks_per_millisecond;
                    _next_due_ticks[idx] = now_ticks + delay;
                }
            }
        }

        private static Globals build_globals_once()
        {
            Snapshot snap = _variable_provider_banked != null ? _variable_provider_banked.Invoke() : null;
            if (snap == null) snap = new Snapshot { Common = new Dictionary<string, object>(), Banks = new Dictionary<string, object>[_bank_count] };
            if (snap.Common == null) snap.Common = new Dictionary<string, object>();
            if (snap.Banks == null || snap.Banks.Length != _bank_count) snap.Banks = new Dictionary<string, object>[_bank_count];

            BankVars[] arr = new BankVars[_bank_count];
            for (int i = 0; i < _bank_count; i++)
            {
                Dictionary<string, object> b = snap.Banks[i] ?? new Dictionary<string, object>();
                arr[i] = new BankVars(b, snap.Common);
            }
            Globals g = new Globals();
            g.Variables = arr;
            return g;
        }

        private static List<int> get_due_indices_nolock()
        {
            long now = DateTime.UtcNow.Ticks;
            int count = _occupied.Count;
            List<int> due = new List<int>(count);
            for (int i = 0; i < count; i++)
            {
                if (!_occupied[i]) continue;
                if (_next_due_ticks[i] <= now) due.Add(i);
            }
            return due;
        }

        private static void recompute_loop_interval_nolock()
        {
            int min_ms = int.MaxValue;
            int count = _occupied.Count;
            for (int i = 0; i < count; i++)
            {
                if (!_occupied[i]) continue;
                int ms = _update_intervals_ms[i];
                if (ms < min_ms) min_ms = ms;
            }
            if (min_ms == int.MaxValue) min_ms = _default_interval_ms;
            if (min_ms < 1) min_ms = 1;
            _loop_interval_ms = min_ms;
            if (_timer != null) _timer.Change(_loop_interval_ms, _loop_interval_ms);
        }

        private static ScriptRunner<bool[]> build_runner()
        {
            string code = build_code();

            ScriptOptions options = ScriptOptions.Default
                .AddReferences(typeof(object).Assembly)
                .AddReferences(typeof(Dictionary<string, object>).Assembly)
                .AddReferences(typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly)
                .AddReferences(typeof(Globals).Assembly)
                .AddImports("System")
                .AddImports("System.Collections.Generic")
                .AddImports("System.Linq")
                .AddImports("Microsoft.CSharp");

            Script<bool[]> script = CSharpScript.Create<bool[]>(code, options, typeof(Globals));

            System.Collections.Immutable.ImmutableArray<Diagnostic> diags = script.Compile();
            bool has_errors = false;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (Diagnostic d in diags)
            {
                if (d.Severity == DiagnosticSeverity.Error)
                {
                    has_errors = true;
                    sb.AppendLine(d.ToString());
                }
            }
            if (has_errors)
            {
                lock (_lock)
                {
                    int countErr = _occupied.Count;
                    for (int i = 0; i < countErr; i++)
                    {
                        if (_occupied[i])
                        {
                            _errors[i] = true;
                            _diagnostics[i] = sb.ToString();
                        }
                    }
                }
                return null;
            }

            try
            {
                ScriptRunner<bool[]> runner = script.CreateDelegate();
                return runner;
            }
            catch (CompilationErrorException ex)
            {
                lock (_lock)
                {
                    int countErr = _occupied.Count;
                    for (int i = 0; i < countErr; i++)
                    {
                        if (_occupied[i])
                        {
                            _errors[i] = true;
                            _diagnostics[i] = string.Join(Environment.NewLine, ex.Diagnostics.Select(x => x.ToString()));
                        }
                    }
                }
                return null;
            }
        }

        private static string build_code()
        {
            List<string> local_conditions;
            List<bool> local_occupied;
            lock (_lock)
            {
                local_conditions = new List<string>(_conditions);
                local_occupied = new List<bool>(_occupied);
            }

            int n = local_conditions.Count;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("bool[] results = new bool[").Append(n.ToString()).Append("];");
            for (int i = 0; i < n; i++)
            {
                if (!local_occupied[i])
                {
                    sb.Append("results[").Append(i.ToString()).Append("]=false;");
                    continue;
                }
                string expr = local_conditions[i] ?? string.Empty;
                string safe = expr.Trim();
                if (safe.Length == 0)
                {
                    sb.Append("results[").Append(i.ToString()).Append("]=false;");
                }
                else
                {
                    sb.Append("try{results[").Append(i.ToString()).Append("]=(bool)(").Append(safe).Append(");}catch{results[").Append(i.ToString()).Append("]=false;}");
                }
            }
            sb.Append("return results;");
            return sb.ToString();
        }
    }
}
