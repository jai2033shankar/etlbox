﻿using ETLBox.ControlFlow;
using ETLBox.Exceptions;
using NLog.Targets;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ETLBox.DataFlow
{
    public abstract class DataFlowExecutableSource<TOutput> : DataFlowSource<TOutput>, IDataFlowExecutableSource<TOutput>
    {
        #region Buffer and completion
        public override ISourceBlock<TOutput> SourceBlock => this.Buffer;
        protected BufferBlock<TOutput> Buffer { get; set; } = new BufferBlock<TOutput>();
        internal override Task BufferCompletion => Buffer.Completion;
        public override void InitBufferObjects()
        {
            Buffer = new BufferBlock<TOutput>(new DataflowBlockOptions()
            {
                BoundedCapacity = MaxBufferSize
            });
            Completion = new Task(
               () =>
               {
                   try
                   {
                       OnExecutionDoAsyncWork();
                       CompleteBufferOnPredecessorCompletion();
                       ErrorSource?.CompleteBufferOnPredecessorCompletion();
                       CleanUpOnSuccess();
                   }
                   catch (Exception e)
                   {
                       FaultBufferOnPredecessorCompletion(e);
                       ErrorSource?.FaultBufferOnPredecessorCompletion(e);
                       CleanUpOnFaulted(e);
                       throw e;
                   }
               }
               , TaskCreationOptions.LongRunning);
        }

        internal override void CompleteBufferOnPredecessorCompletion() => SourceBlock.Complete();
        internal override void FaultBufferOnPredecessorCompletion(Exception e) => SourceBlock.Fault(e);

        #endregion

        #region Execution and IDataFlowExecutableSource
        public virtual void Execute() //remove virtual
        {
            InitNetworkRecursively();
            OnExecutionDoSynchronousWork();
            Completion.RunSynchronously();
        }

        public Task ExecuteAsync()
        {
            InitNetworkRecursively();
            OnExecutionDoSynchronousWork();
            Completion.Start();
            return Completion;
        }

        protected virtual void OnExecutionDoSynchronousWork() { } //abstract

        protected virtual void OnExecutionDoAsyncWork() { } //abstract

        #endregion
    }
}
