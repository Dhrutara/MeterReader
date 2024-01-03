namespace MeterReaderWeb.Services;

using Grpc.Core;
using MeterReaderWeb.Data;
using MeterReaderWeb.Data.Entities;
using MeterReaderWeb.gRPC;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static MeterReaderWeb.gRPC.MeterReadingService;

public class MeterReadingService : MeterReadingServiceBase
{
    private readonly IReadingRepository _repository;
    private readonly ILogger<MeterReadingService> _logger;
    public MeterReadingService(IReadingRepository repository, ILogger<MeterReadingService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    public override async Task<StatusMessage> AddReading(ReadingPacket request, ServerCallContext context)
    {
        if (request.Succesful == ReadingStatus.Success)
        {
            foreach (var r in request.Readings)
            {
                var readingValue = new MeterReading()
                {
                    Value = r.ReadingValue,
                    ReadingDate = r.ReadingTime.ToDateTime(),
                    CustomerId = r.CustomerId
                };

                _repository.AddEntity(readingValue);
            }
        }

        bool isSaved = await _repository.SaveAllAsync();

        if (isSaved)
        {
            return new StatusMessage()
            {
                Succes = ReadingStatus.Success,
                Message = "Successfully saved " + request.Readings.Count + " messages",
            };
        }

        _logger.LogError("Failed to save messages to the database");

        return new StatusMessage()
        {
            Succes = ReadingStatus.Failure,
            Message = "Failed to save messsages",
        };
    }
}
