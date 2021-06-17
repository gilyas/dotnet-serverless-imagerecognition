using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.Core;

namespace Common
{
    public class ImageRecognitionLogger
    {
        [Flags]
        public enum Target { CloudWatchLogs=1, Client=2, All=0xFFFFFFF};

        ILambdaContext _context;
        ILambdaLogger _lambdaLogger;
        ExecutionInput _input;
        CommunicationManager _manager;


        public ImageRecognitionLogger(ExecutionInput input, ILambdaContext context)
        {
            _context = context;
            _lambdaLogger = this._context?.Logger;
            _input = input;

            try
            {
                var connectionTable = Environment.GetEnvironmentVariable("COMMUNICATION_CONNECTION_TABLE");
                context.Logger.LogLine($"Configuring CommunicationManager to use connection table '{connectionTable}'");
                _manager = CommunicationManager.CreateManager(connectionTable);
            }
            catch(Exception e)
            {
                _lambdaLogger.LogLine($"Communication manager failed to initialize: {e.Message}");
            }
        }

        public async Task WriteMessageAsync(string message, Target visibiliy)
        {
            var evnt = new MessageEvent{ Message = message };
            await WriteMessageAsync(evnt, visibiliy);
        }

        public async Task WriteMessageAsync(MessageEvent evnt, Target visibiliy)
        {
            if((visibiliy & Target.CloudWatchLogs) == Target.CloudWatchLogs)
            {
                _lambdaLogger?.LogLine($"{this._context.AwsRequestId}: {evnt.Message}");
            }

            if (_manager != null && (visibiliy & Target.Client) == Target.Client)
            {
                evnt.TargetUser = this._input.UserId;
                evnt.ResourceId = this._input.PhotoId;

                await _manager.SendMessage(evnt);
            }
        }

        public void WriteMessage(string message, Target visibiliy)
        {
            if ((visibiliy & Target.CloudWatchLogs) == Target.CloudWatchLogs)
            {
                _lambdaLogger?.LogLine($"{this._context.AwsRequestId}: {message}");
            }

            if (_manager != null && (visibiliy & Target.Client) == Target.Client)
            {
                var evnt = new MessageEvent(this._input.UserId, this._input.SourceKey) { Message = message };
                _manager.SendMessage(evnt).GetAwaiter().GetResult();
            }
        }
    }
}
