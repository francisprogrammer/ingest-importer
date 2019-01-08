using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using WebApi.Common;
using WebApi.Ingestions;
using IHttpClientFactory = WebApi.Common.IHttpClientFactory;

namespace WebApi.Tests.Ingestions
{
    class IngestionTests
    {
        private IFormFile _formFile;
        private IFile _saveFile;
        private IHttpClientFactory _httpClientFactory;
        private IHttpClient _httpClient;
        private Ingestion _sut;

        [SetUp]
        public void Setup()
        {
            _formFile = Substitute.For<IFormFile>();
            _saveFile = Substitute.For<IFile>();
            _httpClientFactory = Substitute.For<IHttpClientFactory>();
            _httpClient = Substitute.For<IHttpClient>();
            _httpClientFactory.Create().Returns(_httpClient);
            _sut = new Ingestion(_saveFile, _httpClientFactory);
        }

        [Test]
        public async Task Saves_albums_when_no_validation_errors_occur()
        {
            var file = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),
                "../../../Ingestions/single-happy.xlsx"));

            _saveFile
                .GetTempFilePath(_formFile)
                .Returns(file);

            await _sut.Ingest(_formFile);

            await _httpClient
                .Received()
                .SaveAsync("anyUrlToSaveAlbum", Arg.Is<List<Album>>(c =>
                    c.First().AlbumName.Value == "any first album" &&
                    c.First().AlbumName.Location == "2a" &&
                    c.First().AlbumDescription.Value == "any description for album" &&
                    c.First().AlbumDescription.Location == "2b"));

            //await repository
            //    .Received()
            //    .Save(Arg.Is<Album>(album =>
            //        album.Name == "any first album" &&
            //        album.Name.Cell == "2a" &&
            //        album.Description == "any description for album" &&
            //        album.Description.AddedEntities.First().Cell == "2b" &&
            //        album.PublishStatus == "publish" &&
            //        album.PublishStatus.AddedEntities.First().Cell == "2c" &&
            //        album.ReleaseDate == "01/01/2018" &&
            //        album.ReleaseDate.AddedEntities.First().Cell == "2d" &&
            //        album.Works.First().Name == "first work name" &&
            //        album.Works.First().Name.Cell == "2e"));
        }

        [Test]
        public async Task Returns_validation_errors_when_ingestion_fails()
        {
            var file = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),
                "../../../Ingestions/single-sad.xlsx"));

            _saveFile
                .GetTempFilePath(_formFile)
                .Returns(file);

            var result = (BadRequestObjectResult)await _sut.Ingest(_formFile);

            var actualResponse = JsonConvert.SerializeObject(result.Value);
            var expected = JsonConvert.SerializeObject(new
            {
                Errors = new[]
                {
                    new
                    {
                        Name = "albumname",
                        Location = "2a",
                        Message = "Album name required"
                    }
                }
            });

            Assert.That(actualResponse, Is.EqualTo(expected));
        }

        [Test]
        public async Task Album_not_saved_when_there_is_a_validation_error()
        {
            var file = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),
                "../../../Ingestions/single-sad.xlsx"));

            _saveFile
                .GetTempFilePath(_formFile)
                .Returns(file);

            await _sut.Ingest(_formFile);

            await _httpClient
                .DidNotReceive()
                .SaveAsync(Arg.Any<string>(), Arg.Any<List<Album>>());
        }
    }
}
