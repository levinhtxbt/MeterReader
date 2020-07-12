using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using MeterReaderWeb.Services;
using Microsoft.Extensions.Logging;

namespace MeterReaderWorker
{
    public class MessageFactory
    {
        private readonly ILogger<MessageFactory> _logger;

        public MessageFactory(ILogger<MessageFactory> logger)
        {
            _logger = logger;
        }

        public Task<ReadingMessage> Generate(int customerId)
        {
            var reading = new ReadingMessage
            {
                CustomerId = customerId,
                ReadingTime = Timestamp.FromDateTime(DateTime.UtcNow),
                ReadingValue = new Random().Next(1000)
            };

            return Task.FromResult(reading);
        }


    }
}
