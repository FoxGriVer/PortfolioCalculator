using Microsoft.Extensions.Logging;
using Moq;

namespace PortfolioCalculator.Application.Tests.TestHelpers
{
    public static class LoggerMoqExtensions
    {
        public static void VerifyLog(
            this Mock<ILogger> logger,
            LogLevel level,
            string containsMessage,
            Times times)
        {
            logger.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains(containsMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                times);
        }
    }
}
