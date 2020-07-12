using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Grpc.Core;
using Grpc.Net.Client;
using MeterReaderWeb.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeterReaderWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly MessageFactory _messageFactory;
        private MeterReadingService.MeterReadingServiceClient _client;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, MessageFactory messageFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _messageFactory = messageFactory;
        }

        protected MeterReadingService.MeterReadingServiceClient Client
        {
            get
            {
                if (_client == null)
                {
                    var channel = GrpcChannel.ForAddress(_configuration.GetValue<string>("Service:ServerUrl"));
                    _client = new MeterReadingService.MeterReadingServiceClient(channel);
                }

                return _client;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                var customerId = _configuration.GetValue<int>("Service:CustomerId");

                var pkt = new ReadingPacket()
                {
                    Successful = MessageStatus.Success,
                    Notes = "This is our test"
                };

                for (int i = 0; i < 5; ++i)
                    pkt.Readings.Add(await _messageFactory.Generate(customerId));

                var result = await Client.AddReadingAsync(pkt);

                _logger.LogInformation(result.Success == MessageStatus.Success
                    ? "Successfully sent"
                    : "Failed to send");

                await Task.Delay(_configuration.GetValue<int>("Service:DelayInterval"), stoppingToken);
            }
        }
    }
}
