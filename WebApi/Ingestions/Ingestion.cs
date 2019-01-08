
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using OfficeOpenXml;
using WebApi.Common;

namespace WebApi.Ingestions
{
    public class Ingestion : ControllerBase
    {
        private readonly IFile _file;
        private readonly IHttpClientFactory _httpClientFactory;

        public Ingestion(IFile file, IHttpClientFactory httpClientFactory)
        {
            _file = file;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Ingest(IFormFile formFile)
        {
            var filePath = await _file.GetTempFilePath(formFile);

            var file = new FileInfo(filePath);

            var validationErrors = new List<ValidationError>();

            var albums = new List<Album>();

            using (var package = new ExcelPackage(file))
            {
                var worksheet = package.Workbook.Worksheets[1];
                var rowCount = worksheet.Dimension.Rows;
                var colCount = worksheet.Dimension.Columns;

                for (var excelRow = 2; excelRow <= rowCount; excelRow++)
                {
                    var album = new Album();

                    for (var excelCol = 1; excelCol <= colCount; excelCol++)
                    {
                        var name = worksheet.Cells[1, excelCol].Value.ToString();
                        var value = worksheet.Cells[excelRow, excelCol].Value;
                        var location = MapToExcelCelLabel(excelRow, excelCol);

                        if (string.Equals(name, "albumname", StringComparison.OrdinalIgnoreCase))
                        {
                            if (value == null)
                            {
                                validationErrors.Add(new ValidationError(name, location, "Album name required"));
                                continue;
                            }

                            album.AlbumName = new Cell
                            {
                                Value = value.ToString(),
                                Location = location
                            };
                        }

                        if (string.Equals(name, "albumdescription", StringComparison.OrdinalIgnoreCase))
                        {
                            album.AlbumDescription = new Cell
                            {
                                Value = value.ToString(),
                                Location = location
                            };
                        }

                        if (string.Equals(name, "publishstatus", StringComparison.OrdinalIgnoreCase))
                        {
                            album.PublishStatus = new Cell
                            {
                                Value = value.ToString(),
                                Location = location
                            };
                        }

                        if (string.Equals(name, "releasedate", StringComparison.OrdinalIgnoreCase))
                        {
                            var date = value.ToString().Split(' ')[0];
                            album.PublishStatus = new Cell
                            {
                                Value = date,
                                Location = location
                            };
                        }

                        if (string.Equals(name, "workName", StringComparison.OrdinalIgnoreCase))
                        {
                            album.WorkName = new Cell
                            {
                                Value = value.ToString(),
                                Location = location
                            };
                        }
                    }

                    albums.Add(album);
                }


                //    await _repository.Save(album);
            }

            if (validationErrors.Any())
            {
                return new BadRequestObjectResult(new
                {
                    Errors = validationErrors
                });
            }

            using (var client = _httpClientFactory.Create())
            {
                await client.SaveAsync("anyUrlToSaveAlbum", albums);
            }

            return Ok();
        }

        private string MapToExcelCelLabel(int row, int col)
        {
            var colLetters = new[] { "a", "b", "c", "d", "e", "f", "g", "h", "i" };
            return $"{row}{colLetters[col - 1]}";
        }
    }
}