using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using MeterReaderWeb.Data;
using MeterReaderWeb.Data.Entities;
using Microsoft.Extensions.Logging;

namespace MeterReaderWeb.Services
{
    public class MeterService: MeterReadingService.MeterReadingServiceBase
    {
        private readonly ILogger<MeterService> _logger;
        private readonly IReadingRepository _readingRepository;

        public MeterService(ILogger<MeterService> logger, IReadingRepository readingRepository)
        {
            _logger = logger;
            _readingRepository = readingRepository;
        }

        public override async Task<StatusMessage> AddReading(ReadingPacket request, ServerCallContext context)
        {
            var result = new StatusMessage
            {
                Success = MessageStatus.Failure
            };

            if (request.Successful == MessageStatus.Success)
            {
                try
                {
                    foreach (var reading in request.Readings)
                    {
                        _readingRepository.AddEntity(new MeterReading
                        {
                            CustomerId = reading.CustomerId,
                            ReadingDate = reading.ReadingTime.ToDateTime(),
                            Value = reading.ReadingValue
                        });

                        if (await _readingRepository.SaveAllAsync())
                        {
                            result.Success = MessageStatus.Success;
                        }
                    }
                }
                catch (Exception e)
                {
                    result.Message = "Exception throw during process";
                    _logger.LogError($"Exception throw during process {e.Message}");
                }
            }

            return result;
        }
    }
}
