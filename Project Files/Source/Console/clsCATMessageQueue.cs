/*  clsCATMessageQueue.cs

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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CatAtonic;

namespace CATQueueBatching
{
    public sealed class MessageQueueSystem : IDisposable
    {
        public event Action<int, Guid, ScriptCommand> message;
        public event Action<int, bool> queueState;

        public sealed class MessageBatch
        {
            private readonly List<QueuedItem> items;
            private readonly object sync;

            public MessageBatch()
            {
                this.items = new List<QueuedItem>();
                this.sync = new object();
            }

            public Guid add(ScriptCommand cmd)
            {
                Guid id = Guid.NewGuid();
                lock (this.sync)
                {
                    this.items.Add(new QueuedItem(id, cmd));
                }
                return id;
            }

            internal bool isEmpty()
            {
                lock (this.sync)
                {
                    return this.items.Count == 0;
                }
            }

            internal QueuedItem[] snapshot()
            {
                lock (this.sync)
                {
                    return this.items.ToArray();
                }
            }
        }

        internal struct QueuedItem
        {
            public readonly Guid id;
            public readonly ScriptCommand command;

            public QueuedItem(Guid id, ScriptCommand command)
            {
                this.id = id;
                this.command = command;
            }
        }

        private readonly BlockingCollection<QueuedItem>[] queues;
        private readonly CancellationTokenSource[] cts_array;
        private readonly Task[] workers;
        private readonly bool[] running;
        private readonly bool[] busy;
        private readonly object[] queue_locks;
        private readonly int queue_count;
        private int disposed;

        public MessageQueueSystem(int n)
        {
            if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n));

            this.queue_count = n;
            this.queues = new BlockingCollection<QueuedItem>[n];
            this.cts_array = new CancellationTokenSource[n];
            this.workers = new Task[n];
            this.running = new bool[n];
            this.busy = new bool[n];
            this.queue_locks = new object[n];

            for (int i = 0; i < n; i++)
            {
                this.queues[i] = new BlockingCollection<QueuedItem>(new ConcurrentQueue<QueuedItem>());
                this.cts_array[i] = new CancellationTokenSource();
                this.running[i] = false;
                this.busy[i] = false;
                this.queue_locks[i] = new object();
                this.startQueue(i);
            }
        }

        public MessageBatch createBatch()
        {
            return new MessageBatch();
        }

        public void sendBatch(int queue_index, MessageBatch batch)
        {
            this.throwIfDisposed();
            if (queue_index < 0 || queue_index >= this.queue_count) throw new ArgumentOutOfRangeException(nameof(queue_index));
            if (batch == null) throw new ArgumentNullException(nameof(batch));
            QueuedItem[] arr = batch.snapshot();
            if (arr.Length == 0) return;
            this.ensureQueueRunning(queue_index);
            for (int i = 0; i < arr.Length; i++)
            {
                this.queues[queue_index].Add(arr[i]);
            }
        }

        public bool isBusy(int queue_index)
        {
            this.throwIfDisposed();
            if (queue_index < 0 || queue_index >= this.queue_count) throw new ArgumentOutOfRangeException(nameof(queue_index));
            return this.busy[queue_index];
        }

        public int getPending(int queue_index)
        {
            this.throwIfDisposed();
            if (queue_index < 0 || queue_index >= this.queue_count) throw new ArgumentOutOfRangeException(nameof(queue_index));
            return this.queues[queue_index].Count;
        }

        public bool isEmpty(int queue_index)
        {
            this.throwIfDisposed();
            if (queue_index < 0 || queue_index >= this.queue_count) throw new ArgumentOutOfRangeException(nameof(queue_index));
            return this.queues[queue_index].Count == 0 && !this.busy[queue_index];
        }

        public void stopAndClearQueue(int queue_index)
        {
            this.throwIfDisposed();
            if (queue_index < 0 || queue_index >= this.queue_count) throw new ArgumentOutOfRangeException(nameof(queue_index));
            bool was_running;
            lock (this.queue_locks[queue_index])
            {
                was_running = this.running[queue_index];
                if (was_running) this.cts_array[queue_index].Cancel();
            }
            if (!was_running) return;
            try { this.workers[queue_index]?.Wait(); } catch { }
            lock (this.queue_locks[queue_index])
            {
                this.drainQueue(queue_index);
                this.busy[queue_index] = false;
                this.running[queue_index] = false;
            }
            Action<int, bool> qs = this.queueState;
            if (qs != null) qs(queue_index, false);
        }

        public void stopAll()
        {
            this.throwIfDisposed();
            for (int i = 0; i < this.queue_count; i++)
            {
                this.stopAndClearQueue(i);
            }
        }

        private void startQueue(int index)
        {
            lock (this.queue_locks[index])
            {
                if (this.running[index]) return;
                if (this.cts_array[index].IsCancellationRequested)
                {
                    this.cts_array[index].Dispose();
                    this.cts_array[index] = new CancellationTokenSource();
                }
                CancellationToken token = this.cts_array[index].Token;
                this.workers[index] = Task.Factory.StartNew(() => this.workerLoop(index, token), token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                this.running[index] = true;
            }
            Action<int, bool> qs = this.queueState;
            if (qs != null) qs(index, true);
        }

        private void ensureQueueRunning(int index)
        {
            if (!this.running[index]) this.startQueue(index);
        }

        private void workerLoop(int index, CancellationToken token)
        {
            BlockingCollection<QueuedItem> q = this.queues[index];
            try
            {
                while (true)
                {
                    if (token.IsCancellationRequested) break;

                    QueuedItem item;
                    try
                    {
                        item = q.Take(token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    this.busy[index] = true;

                    ScriptCommand cmd = item.command;
                    if (cmd != null && cmd.type == ScriptCommandType.Wait)
                    {
                        int ms = cmd.wait_ms;
                        if (ms > 0)
                        {
                            bool cancelled = token.WaitHandle.WaitOne(ms);
                            if (cancelled) break;
                        }
                    }
                    else
                    {
                        Action<int, Guid, ScriptCommand> m = this.message;
                        if (m != null) m(index, item.id, cmd);
                    }

                    if (q.Count == 0) this.busy[index] = false;
                }
            }
            finally
            {
                this.busy[index] = false;
            }
        }

        private void drainQueue(int index)
        {
            BlockingCollection<QueuedItem> q = this.queues[index];
            QueuedItem tmp;
            while (q.TryTake(out tmp)) { }
        }

        private void throwIfDisposed()
        {
            if (this.disposed != 0) throw new ObjectDisposedException(nameof(MessageQueueSystem));
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref this.disposed, 1) != 0) return;
            for (int i = 0; i < this.queue_count; i++)
            {
                try { this.cts_array[i].Cancel(); } catch { }
            }
            for (int i = 0; i < this.queue_count; i++)
            {
                try { this.workers[i]?.Wait(); } catch { }
            }
            for (int i = 0; i < this.queue_count; i++)
            {
                try { this.queues[i]?.Dispose(); } catch { }
                try { this.cts_array[i]?.Dispose(); } catch { }
            }
        }

        public void dispose()
        {
            this.Dispose();
        }
    }
}
