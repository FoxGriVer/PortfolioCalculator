using MediatR;

namespace PortfolioCalculator.Application.Import
{
    public sealed class ImportAllCsvCommand : IRequest<ImportAllCsvResult>
    {
        public string FolderPath { get; }

        public ImportAllCsvCommand(string folderPath)
        {
            FolderPath = folderPath;
        }
    }
}
