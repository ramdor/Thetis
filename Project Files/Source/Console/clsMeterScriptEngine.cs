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
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        private sealed class CompileResult
        {
            public ScriptRunner<bool> Runner;
            public string Diagnostics;
            public bool Success;
        }

        private struct EvalResult
        {
            public int Index;
            public bool Value;
            public bool Error;
            public string Diagnostic;
        }

        private static readonly object _lock = new object();

        private static readonly ScriptOptions _script_options =
            ScriptOptions.Default
                .AddReferences(typeof(object).Assembly)
                .AddReferences(typeof(Dictionary<string, object>).Assembly)
                .AddReferences(typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly)
                .AddReferences(typeof(Globals).Assembly)
                .AddImports("System")
                .AddImports("System.Collections.Generic")
                .AddImports("System.Linq")
                .AddImports("Microsoft.CSharp");

        private static List<string> _conditions = new List<string>();
        private static List<bool> _occupied = new List<bool>();
        private static List<int> _update_intervals_ms = new List<int>();
        private static List<long> _next_due_ticks = new List<long>();
        private static List<bool> _results = new List<bool>();
        private static List<bool> _errors = new List<bool>();
        private static List<string> _diagnostics = new List<string>();
        private static Queue<int> _free_indices = new Queue<int>();

        private static List<ScriptRunner<bool>> _delegates = new List<ScriptRunner<bool>>();
        private static List<bool> _needs_compile = new List<bool>();
        private static Dictionary<string, ScriptRunner<bool>> _delegate_cache = new Dictionary<string, ScriptRunner<bool>>();

        private static bool _needs_recompile = false;

        private static Timer _timer = null;
        private static int _default_interval_ms = 100;
        private static int _loop_interval_ms = 100;
        private static int _bank_count = 1;
        private static Func<Snapshot> _variable_provider_banked = null;
        private static readonly long _ticks_per_millisecond = TimeSpan.TicksPerMillisecond;
        private static int _batch_depth = 0;
        private static bool _loop_interval_dirty = false;

        private static Thread _compile_thread = null;
        private static AutoResetEvent _compile_event = new AutoResetEvent(false);
        private static bool _stopping = false;
        private static int _compile_debounce_ms = 25;

        public static void Start(Func<Snapshot> variable_provider_banked, int default_interval_ms, int bank_count)
        {
            lock (_lock)
            {
                _variable_provider_banked = variable_provider_banked;
                _bank_count = bank_count < 1 ? 1 : bank_count;
                _default_interval_ms = default_interval_ms < 1 ? 1 : default_interval_ms;
                _loop_interval_ms = _default_interval_ms;
                _stopping = false;
                if (_timer == null)
                {
                    _timer = new Timer(_ => tick(), null, _loop_interval_ms, _loop_interval_ms);
                }
                else
                {
                    _timer.Change(_loop_interval_ms, _loop_interval_ms);
                }
                if (_compile_thread == null || !_compile_thread.IsAlive)
                {
                    _compile_thread = new Thread(compile_worker_entry);
                    _compile_thread.IsBackground = true;
                    _compile_thread.Priority = ThreadPriority.BelowNormal;
                    _compile_thread.Name = "MeterScriptEngine-Compiler";
                    _compile_thread.Start();
                }
            }
            Thread warm = new Thread(warmup_roslyn_entry);
            warm.IsBackground = true;
            warm.Priority = ThreadPriority.BelowNormal;
            warm.Name = "MeterScriptEngine-Warmup";
            warm.Start();
        }

        public static void Stop()
        {
            lock (_lock)
            {
                _stopping = true;
                _compile_event.Set();
                if (_timer != null)
                {
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    _timer.Dispose();
                    _timer = null;
                }
            }
        }

        public static bool IsInBatch
        {
            get
            {
                bool in_batch;
                lock (_lock)
                {
                    in_batch = _batch_depth > 0;
                }
                return in_batch;
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
            bool do_recompute = false;
            bool schedule_compile = false;
            lock (_lock)
            {
                if (_batch_depth > 0) _batch_depth--;
                if (_batch_depth == 0)
                {
                    if (_needs_recompile) schedule_compile = true;
                    if (_loop_interval_dirty) do_recompute = true;
                    _loop_interval_dirty = false;
                }
            }
            if (do_recompute)
            {
                lock (_lock)
                {
                    recompute_loop_interval_nolock();
                }
            }
            if (schedule_compile)
            {
                lock (_lock)
                {
                    schedule_compile_if_needed_nolock();
                }
            }
        }

        public static int RegisterLed()
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
                    _delegates[index] = null;
                    _needs_compile[index] = false;
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
                    _delegates.Add(null);
                    _needs_compile.Add(false);
                }
                _loop_interval_dirty = true;
                if (_batch_depth == 0)
                {
                    recompute_loop_interval_nolock();
                }
                return index;
            }
        }

        public static void UnregisterLed(int index)
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
                _delegates[index] = null;
                _needs_compile[index] = false;
                _free_indices.Enqueue(index);
                _loop_interval_dirty = true;
                if (_batch_depth == 0)
                {
                    recompute_loop_interval_nolock();
                }
            }
        }

        //public static bool SetCondition(int index, string condition)
        //{
        //    if (index < 0 || index >= _occupied.Count) return false;
        //    if (!_occupied[index]) return false;
        //    string expr = condition ?? string.Empty;
        //    lock (_lock)
        //    {
        //        _conditions[index] = expr;
        //        _diagnostics[index] = string.Empty;
        //        _errors[index] = false;
        //        _needs_compile[index] = true;
        //        _needs_recompile = true;
        //        if (_batch_depth == 0) schedule_compile_if_needed_nolock();
        //    }
        //    return true;
        //}
        public static bool SetCondition(int index, string condition)
        {
            if (index < 0 || index >= _occupied.Count) return false;
            if (!_occupied[index]) return false;

            string expr = condition ?? string.Empty;
            string trimmed = expr.Trim();

            if (trimmed.Length == 0)
            {
                lock (_lock)
                {
                    _conditions[index] = string.Empty;
                    _delegates[index] = null;
                    _needs_compile[index] = false;
                    _errors[index] = false;
                    _diagnostics[index] = string.Empty;
                }
                return true;
            }

            ExpressionSyntax node = SyntaxFactory.ParseExpression(trimmed);
            bool has_error = false;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (Diagnostic d in node.GetDiagnostics())
            {
                if (d.Severity == DiagnosticSeverity.Error)
                {
                    has_error = true;
                    sb.AppendLine(d.ToString());
                }
            }

            if (has_error)
            {
                lock (_lock)
                {
                    _conditions[index] = expr;
                    _delegates[index] = null;
                    _needs_compile[index] = false;
                    _errors[index] = true;
                    _diagnostics[index] = sb.ToString();
                }
                return false;
            }

            lock (_lock)
            {
                _conditions[index] = expr;
                _diagnostics[index] = string.Empty;
                _errors[index] = false;
                _needs_compile[index] = true;
                _needs_recompile = true;
                if (_batch_depth == 0) schedule_compile_if_needed_nolock();
            }
            return true;
        }
        public static void SetUpdateInterval(int index, int milliseconds)
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

        public static bool ReadResult(int index)
        {
            lock (_lock)
            {
                if (index < 0 || index >= _occupied.Count) return false;
                if (!_occupied[index]) return false;
                return _results[index];
            }
        }

        public static bool ReadError(int index)
        {
            lock (_lock)
            {
                if (index < 0 || index >= _occupied.Count) return true;
                if (!_occupied[index]) return true;
                return _errors[index];
            }
        }

        public static string ReadDiagnostic(int index)
        {
            lock (_lock)
            {
                if (index < 0 || index >= _occupied.Count) return string.Empty;
                if (!_occupied[index]) return string.Empty;
                return _diagnostics[index] ?? string.Empty;
            }
        }

        public static void SetLoopIntervalFloor(int milliseconds)
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
            List<int> due_indices = null;
            List<ScriptRunner<bool>> due_delegates = null;

            lock (_lock)
            {
                if (_needs_recompile && _batch_depth == 0) schedule_compile_if_needed_nolock();
                due_indices = get_due_indices_nolock();
                int n = due_indices.Count;
                due_delegates = new List<ScriptRunner<bool>>(n);
                for (int i = 0; i < n; i++)
                {
                    int idx = due_indices[i];
                    ScriptRunner<bool> d = _delegates[idx];
                    due_delegates.Add(d);
                }
            }

            if (due_indices == null || due_indices.Count == 0) return;

            Globals globals = build_globals_once();

            List<EvalResult> outputs = new List<EvalResult>(due_indices.Count);
            for (int i = 0; i < due_indices.Count; i++)
            {
                int idx = due_indices[i];
                ScriptRunner<bool> del = due_delegates[i];
                if (del == null)
                {
                    EvalResult er0 = new EvalResult();
                    er0.Index = idx;
                    er0.Value = false;
                    er0.Error = false;
                    er0.Diagnostic = string.Empty;
                    outputs.Add(er0);
                    continue;
                }
                try
                {
                    bool r = del(globals).GetAwaiter().GetResult();
                    EvalResult er1 = new EvalResult();
                    er1.Index = idx;
                    er1.Value = r;
                    er1.Error = false;
                    er1.Diagnostic = string.Empty;
                    outputs.Add(er1);
                }
                catch (Exception ex)
                {
                    EvalResult er2 = new EvalResult();
                    er2.Index = idx;
                    er2.Value = false;
                    er2.Error = true;
                    er2.Diagnostic = ex.Message ?? string.Empty;
                    outputs.Add(er2);
                }
            }

            long now_ticks = DateTime.UtcNow.Ticks;

            lock (_lock)
            {
                for (int i = 0; i < outputs.Count; i++)
                {
                    EvalResult er = outputs[i];
                    if (er.Index < 0 || er.Index >= _occupied.Count) continue;
                    if (!_occupied[er.Index]) continue;
                    _results[er.Index] = er.Value;
                    if (er.Error)
                    {
                        _errors[er.Index] = true;
                        _diagnostics[er.Index] = er.Diagnostic;
                    }
                    long delay = (long)_update_intervals_ms[er.Index] * _ticks_per_millisecond;
                    _next_due_ticks[er.Index] = now_ticks + delay;
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

        private static void schedule_compile_if_needed_nolock()
        {
            if (_stopping) return;
            if (_batch_depth > 0) return;
            _compile_event.Set();
        }

        private static void compile_worker_entry()
        {
            while (true)
            {
                _compile_event.WaitOne();
                if (_stopping) break;
                if (_compile_debounce_ms > 0) Thread.Sleep(_compile_debounce_ms);

                while (true)
                {
                    int[] indices;
                    string[] exprs;

                    lock (_lock)
                    {
                        if (_stopping) return;
                        if (_batch_depth > 0) break;
                        List<int> list = new List<int>();
                        List<string> exprlist = new List<string>();
                        int count = _conditions.Count;
                        for (int i = 0; i < count; i++)
                        {
                            if (_occupied[i] && _needs_compile[i])
                            {
                                list.Add(i);
                                exprlist.Add(_conditions[i]);
                                _needs_compile[i] = false;
                            }
                        }
                        if (list.Count == 0)
                        {
                            _needs_recompile = false;
                            break;
                        }
                        indices = list.ToArray();
                        exprs = exprlist.ToArray();
                    }

                    for (int k = 0; k < indices.Length; k++)
                    {
                        int idx = indices[k];
                        string expr = exprs[k] ?? string.Empty;
                        string trimmed = expr.Trim();

                        if (trimmed.Length == 0)
                        {
                            lock (_lock)
                            {
                                if (idx >= 0 && idx < _delegates.Count)
                                {
                                    if (_occupied[idx] && _conditions[idx] == expr)
                                    {
                                        _delegates[idx] = null;
                                        _errors[idx] = false;
                                        _diagnostics[idx] = string.Empty;
                                    }
                                }
                            }
                            continue;
                        }

                        ScriptRunner<bool> cached;
                        bool found = false;
                        lock (_lock)
                        {
                            found = _delegate_cache.TryGetValue(trimmed, out cached);
                        }
                        if (found && cached != null)
                        {
                            lock (_lock)
                            {
                                if (_occupied[idx] && _conditions[idx] == expr)
                                {
                                    _delegates[idx] = cached;
                                    _errors[idx] = false;
                                    _diagnostics[idx] = string.Empty;
                                }
                            }
                            continue;
                        }

                        CompileResult cr = compile_one(trimmed);

                        lock (_lock)
                        {
                            if (!_occupied[idx]) continue;
                            if (_conditions[idx] != expr) { _needs_compile[idx] = true; _needs_recompile = true; continue; }
                            if (cr.Success && cr.Runner != null)
                            {
                                _delegates[idx] = cr.Runner;
                                if (!_delegate_cache.ContainsKey(trimmed)) _delegate_cache[trimmed] = cr.Runner;
                                _errors[idx] = false;
                                _diagnostics[idx] = string.Empty;
                            }
                            else
                            {
                                _errors[idx] = true;
                                _diagnostics[idx] = cr.Diagnostics ?? string.Empty;
                            }
                        }
                    }
                }
            }
        }

        private static CompileResult compile_one(string expr_trimmed)
        {
            string code = "return (bool)(" + expr_trimmed + ");";
            Script<bool> script = CSharpScript.Create<bool>(code, _script_options, typeof(Globals));
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
                CompileResult cr1 = new CompileResult();
                cr1.Runner = null;
                cr1.Diagnostics = sb.ToString();
                cr1.Success = false;
                return cr1;
            }
            try
            {
                ScriptRunner<bool> r = script.CreateDelegate();
                CompileResult cr2 = new CompileResult();
                cr2.Runner = r;
                cr2.Diagnostics = string.Empty;
                cr2.Success = true;
                return cr2;
            }
            catch (CompilationErrorException ex)
            {
                string diag = string.Join(Environment.NewLine, ex.Diagnostics.Select(x => x.ToString()));
                CompileResult cr3 = new CompileResult();
                cr3.Runner = null;
                cr3.Diagnostics = diag;
                cr3.Success = false;
                return cr3;
            }
        }

        private static void warmup_roslyn_entry()
        {
            try
            {
                Script<bool> s = CSharpScript.Create<bool>("return true;", _script_options, typeof(Globals));
                s.Compile();
                ScriptRunner<bool> r = s.CreateDelegate();
                Globals g = new Globals();
                g.Variables = new BankVars[0];
                bool b = r(g).GetAwaiter().GetResult();
            }
            catch { }
        }
    }
}

