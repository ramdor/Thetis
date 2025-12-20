/*  clsCatAtonic.cs

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
using System.Globalization;

namespace CatAtonic
{
    public enum ScriptCommandType
    {
        CatMessage,
        CatMessageVar,
        Wait,
        CatState
    }

    public class ScriptCommand
    {
        public ScriptCommandType type;
        public string text;
        public int wait_ms;
        public string variable_name;
        public List<string> guard_true;
        public List<string> guard_false;
        public string ID;
        public int macro;
        public int button_index;
    }

    public class ScriptResult
    {
        public List<ScriptCommand> commands;
        public bool is_valid;
        public string error_message;

        public ScriptResult()
        {
            this.commands = new List<ScriptCommand>();
            this.is_valid = true;
            this.error_message = string.Empty;
        }
    }

    public enum TokenType
    {
        Bracket,
        Cat,
        Eof,
        Error
    }

    public class Token
    {
        public TokenType type;
        public string text;

        public Token(TokenType type, string text)
        {
            this.type = type;
            this.text = text;
        }
    }

    public class Tokeniser
    {
        private readonly string input;
        private int index;
        private readonly int length;

        public Tokeniser(string input)
        {
            this.input = input ?? string.Empty;
            this.index = 0;
            this.length = this.input.Length;
        }

        private void skip_ws_and_comments()
        {
            while (true)
            {
                while (this.index < this.length && char.IsWhiteSpace(this.input[this.index])) this.index++;
                if (this.index >= this.length) return;
                if (this.input[this.index] == '#')
                {
                    while (this.index < this.length && this.input[this.index] != '\n') this.index++;
                    if (this.index < this.length && this.input[this.index] == '\n') this.index++;
                    continue;
                }
                return;
            }
        }

        public Token next()
        {
            this.skip_ws_and_comments();
            if (this.index >= this.length) return new Token(TokenType.Eof, string.Empty);

            char c = this.input[this.index];
            if (c == '[')
            {
                int start = this.index + 1;
                int j = start;
                while (j < this.length)
                {
                    char ch = this.input[j];
                    if (ch == ']')
                    {
                        string inner = this.input.Substring(start, j - start).Trim();
                        this.index = j + 1;
                        return new Token(TokenType.Bracket, inner);
                    }
                    if (ch == '[' || ch == ';' || ch == '\r' || ch == '\n')
                    {
                        string inner_so_far = this.input.Substring(start, j - start).Trim();
                        if (inner_so_far.Length == 0) inner_so_far = "?";
                        return new Token(TokenType.Error, "non completed [] at " + inner_so_far);
                    }
                    j++;
                }
                return new Token(TokenType.Error, "non completed []");
            }
            else
            {
                int j = this.index;
                while (j < this.length)
                {
                    char ch = this.input[j];
                    if (ch == ';')
                    {
                        string cmd = this.input.Substring(this.index, j - this.index + 1).Trim();
                        this.index = j + 1;
                        return new Token(TokenType.Cat, cmd);
                    }
                    if (ch == '\r' || ch == '\n' || ch == '[' || ch == ']' || ch == '#')
                    {
                        return new Token(TokenType.Error, "non terminated cat message in a ;");
                    }
                    j++;
                }
                return new Token(TokenType.Error, "non terminated cat message in a ;");
            }
        }
    }

    public class CATScriptInterpreter
    {
        private struct if_ctx
        {
            public List<string> conds_seen;
            public string branch_cond;
            public bool seen_else;
            public HashSet<string> used_conds;
        }

        public CATScriptInterpreter()
        {
        }

        public ScriptResult run(string script)
        {
            ScriptResult result = new ScriptResult();
            Tokeniser t = new Tokeniser(script);
            List<if_ctx> stack = new List<if_ctx>();
            bool expect_cat_after_cat_state = false;
            string pending_var_name = null;

            while (true)
            {
                Token tok = t.next();
                if (tok.type == TokenType.Eof) break;
                if (tok.type == TokenType.Error)
                {
                    result.is_valid = false;
                    result.error_message = tok.text;
                    return result;
                }

                if (tok.type == TokenType.Bracket)
                {
                    if (expect_cat_after_cat_state)
                    {
                        result.is_valid = false;
                        result.error_message = "STATE must be followed by a CAT command";
                        return result;
                    }

                    string inner = tok.text.Trim();
                    string upper = inner.ToUpperInvariant();

                    if (upper == "ELSE")
                    {
                        if (stack.Count == 0)
                        {
                            result.is_valid = false;
                            result.error_message = "ELSE without IF";
                            return result;
                        }
                        if_ctx ctx = stack[stack.Count - 1];
                        if (ctx.seen_else)
                        {
                            result.is_valid = false;
                            result.error_message = "multiple ELSE in IF";
                            return result;
                        }
                        ctx.seen_else = true;
                        ctx.branch_cond = null;
                        stack[stack.Count - 1] = ctx;
                        continue;
                    }

                    if (upper.StartsWith("ELSE_IF_", StringComparison.Ordinal) || upper.StartsWith("ELSEIF_", StringComparison.Ordinal))
                    {
                        if (stack.Count == 0)
                        {
                            result.is_valid = false;
                            result.error_message = "ELSE_IF without IF";
                            return result;
                        }
                        if_ctx ctx = stack[stack.Count - 1];
                        if (ctx.seen_else)
                        {
                            result.is_valid = false;
                            result.error_message = "ELSE_IF after ELSE";
                            return result;
                        }
                        int prefix_len = upper.StartsWith("ELSEIF_", StringComparison.Ordinal) ? 7 : 8;
                        string cond_name = inner.Substring(prefix_len);
                        string cond_upper = cond_name.ToUpperInvariant();
                        if (ctx.used_conds != null && ctx.used_conds.Contains(cond_upper))
                        {
                            result.is_valid = false;
                            result.error_message = "duplicate condition in IF chain: " + cond_name;
                            return result;
                        }
                        if (ctx.used_conds == null) ctx.used_conds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        ctx.used_conds.Add(cond_upper);
                        if (ctx.conds_seen == null) ctx.conds_seen = new List<string>();
                        ctx.conds_seen.Add(cond_name);
                        ctx.branch_cond = cond_name;
                        stack[stack.Count - 1] = ctx;
                        continue;
                    }

                    if (upper == "END" || upper == "ENDIF")
                    {
                        if (stack.Count == 0)
                        {
                            result.is_valid = false;
                            result.error_message = "END without IF";
                            return result;
                        }
                        stack.RemoveAt(stack.Count - 1);
                        continue;
                    }

                    if (upper == "STATE")
                    {
                        expect_cat_after_cat_state = true;
                        continue;
                    }

                    if (upper.StartsWith("VAR_", StringComparison.Ordinal))
                    {
                        if (pending_var_name != null)
                        {
                            result.is_valid = false;
                            result.error_message = "multiple VAR before CAT";
                            return result;
                        }
                        pending_var_name = inner.Substring(4);
                        if (pending_var_name.Length == 0)
                        {
                            result.is_valid = false;
                            result.error_message = "empty VAR name";
                            return result;
                        }
                        continue;
                    }

                    if (upper == "WAIT")
                    {
                        ScriptCommand cmd_w = new ScriptCommand();
                        cmd_w.type = ScriptCommandType.Wait;
                        cmd_w.text = "[WAIT]";
                        cmd_w.wait_ms = 100;
                        cmd_w.variable_name = null;
                        set_guard_from_stack(cmd_w, stack);
                        result.commands.Add(cmd_w);
                        continue;
                    }

                    if (upper.StartsWith("WAIT", StringComparison.Ordinal))
                    {
                        string digits = inner.Substring(4);
                        int ms = 0;
                        bool ok = int.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out ms);
                        if (!ok || ms < 0)
                        {
                            result.is_valid = false;
                            result.error_message = "invalid WAIT value";
                            return result;
                        }
                        ScriptCommand cmd_wv = new ScriptCommand();
                        cmd_wv.type = ScriptCommandType.Wait;
                        cmd_wv.text = "[WAIT" + ms.ToString(CultureInfo.InvariantCulture) + "]";
                        cmd_wv.wait_ms = ms;
                        cmd_wv.variable_name = null;
                        set_guard_from_stack(cmd_wv, stack);
                        result.commands.Add(cmd_wv);
                        continue;
                    }

                    if (upper.StartsWith("IF_", StringComparison.Ordinal))
                    {
                        string cond_name = inner.Substring(3);
                        string cond_upper = cond_name.ToUpperInvariant();
                        if_ctx ctx_if = new if_ctx();
                        ctx_if.conds_seen = new List<string>();
                        ctx_if.conds_seen.Add(cond_name);
                        ctx_if.branch_cond = cond_name;
                        ctx_if.seen_else = false;
                        ctx_if.used_conds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        ctx_if.used_conds.Add(cond_upper);
                        stack.Add(ctx_if);
                        continue;
                    }

                    result.is_valid = false;
                    result.error_message = "unknown command in []: " + inner;
                    return result;
                }

                if (tok.type == TokenType.Cat)
                {
                    ScriptCommand cmd = new ScriptCommand();

                    if (expect_cat_after_cat_state)
                    {
                        cmd.type = ScriptCommandType.CatState;
                        cmd.text = tok.text;
                        cmd.wait_ms = 0;
                        cmd.variable_name = pending_var_name;
                        pending_var_name = null;
                        expect_cat_after_cat_state = false;
                    }
                    else
                    {
                        if (pending_var_name != null)
                        {
                            cmd.type = ScriptCommandType.CatMessageVar;
                            cmd.text = tok.text;
                            cmd.wait_ms = 0;
                            cmd.variable_name = pending_var_name;
                            pending_var_name = null;
                        }
                        else
                        {
                            cmd.type = ScriptCommandType.CatMessage;
                            cmd.text = tok.text;
                            cmd.wait_ms = 0;
                            cmd.variable_name = null;
                        }
                    }

                    set_guard_from_stack(cmd, stack);
                    result.commands.Add(cmd);
                    continue;
                }
            }

            if (stack.Count != 0)
            {
                result.is_valid = false;
                result.error_message = "missing [END] to a starting [IF]";
                return result;
            }

            if (expect_cat_after_cat_state)
            {
                result.is_valid = false;
                result.error_message = "STATE without following CAT command";
                return result;
            }

            if (pending_var_name != null)
            {
                result.is_valid = false;
                result.error_message = "VAR without following CAT command";
                return result;
            }

            return result;
        }

        public List<ScriptCommand> filter_now(ScriptResult r, Func<string, bool> eval)
        {
            List<ScriptCommand> list = new List<ScriptCommand>();
            if (r == null || !r.is_valid) return list;
            int i = 0;
            int n = r.commands.Count;
            bool cat_state_done = false;
            while (i < n)
            {
                ScriptCommand c = r.commands[i];
                if (guard_holds(c, eval))
                {
                    if (c.type == ScriptCommandType.CatState)
                    {
                        if (cat_state_done) return new List<ScriptCommand>();
                        cat_state_done = true;
                    }
                    list.Add(c);
                }
                i++;
            }
            return list;
        }

        public int total_wait_milliseconds_now(ScriptResult r, Func<string, bool> eval)
        {
            if (r == null || !r.is_valid) return 0;
            int total = 0;
            int i = 0;
            int n = r.commands.Count;
            while (i < n)
            {
                ScriptCommand c = r.commands[i];
                if (c.type == ScriptCommandType.Wait && guard_holds(c, eval)) total += c.wait_ms;
                i++;
            }
            return total;
        }

        public string cat_state_command_now(ScriptResult r, Func<string, bool> eval)
        {
            if (r == null || !r.is_valid) return string.Empty;
            int i = 0;
            int n = r.commands.Count;
            while (i < n)
            {
                ScriptCommand c = r.commands[i];
                if (c.type == ScriptCommandType.CatState && guard_holds(c, eval)) return c.text;
                i++;
            }
            return string.Empty;
        }

        private static bool guard_holds(ScriptCommand c, Func<string, bool> eval)
        {
            if (c.guard_true != null)
            {
                int i = 0;
                int n = c.guard_true.Count;
                while (i < n)
                {
                    string name = c.guard_true[i];
                    if (!eval(name)) return false;
                    i++;
                }
            }
            if (c.guard_false != null)
            {
                int j = 0;
                int m = c.guard_false.Count;
                while (j < m)
                {
                    string name = c.guard_false[j];
                    if (eval(name)) return false;
                    j++;
                }
            }
            return true;
        }

        private static void set_guard_from_stack(ScriptCommand cmd, List<if_ctx> stack)
        {
            List<string> gtrue = null;
            List<string> gfalse = null;

            int i = 0;
            int n = stack.Count;
            while (i < n)
            {
                if_ctx ctx = stack[i];
                if (ctx.branch_cond == null)
                {
                    if (ctx.conds_seen != null)
                    {
                        int k = 0;
                        int t = ctx.conds_seen.Count;
                        while (k < t)
                        {
                            if (gfalse == null) gfalse = new List<string>();
                            if (!contains_string(gfalse, ctx.conds_seen[k])) gfalse.Add(ctx.conds_seen[k]);
                            k++;
                        }
                    }
                }
                else
                {
                    if (gtrue == null) gtrue = new List<string>();
                    if (!contains_string(gtrue, ctx.branch_cond)) gtrue.Add(ctx.branch_cond);

                    if (ctx.conds_seen != null)
                    {
                        int k2 = 0;
                        int t2 = ctx.conds_seen.Count - 1;
                        while (k2 < t2)
                        {
                            if (gfalse == null) gfalse = new List<string>();
                            if (!contains_string(gfalse, ctx.conds_seen[k2])) gfalse.Add(ctx.conds_seen[k2]);
                            k2++;
                        }
                    }
                }
                i++;
            }

            cmd.guard_true = gtrue ?? new List<string>();
            cmd.guard_false = gfalse ?? new List<string>();
        }

        private static bool contains_string(List<string> list, string s)
        {
            int i = 0;
            int n = list.Count;
            while (i < n)
            {
                if (string.Equals(list[i], s, StringComparison.OrdinalIgnoreCase)) return true;
                i++;
            }
            return false;
        }
    }
}
