using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ReferenceDataApi.Controllers;
using ReferenceDataApi.Dtos;
using ReferenceDataApi.Repositories;
using ReferenceDataApi.Services;

namespace ReferenceDataApi.IntegrationTests
{
    public class ReferenceDataControllerTests
    {
        private ReferenceDataService _referenceDataService;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _referenceDataService = new ReferenceDataService(new InMemoryReferenceDataRepository());
            _fixture = new Fixture();
        }

        private static void ResponsesAreEquivalent(ReferenceDataResponse expected, ReferenceDataResponse actual)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(actual!.Id, Is.EqualTo(expected.Id));
                Assert.That(actual.Label, Is.EqualTo(expected.Label));
                Assert.That(actual.DerivesFrom, Is.EqualTo(expected.DerivesFrom));
                Assert.That(actual.Active, Is.EqualTo(expected.Active));
                Assert.That(actual.ReferenceDataType, Is.EqualTo(expected.ReferenceDataType));
                Assert.That(actual.CreatedDate, Is.EqualTo(expected.CreatedDate));
            }
        }

         [Test]
        public void IT_01_Get_ReferenceDataById_ReturnsOkObjectResult_WithExpectedResponse_WhenEntityExists()
        {
            // Arrange

            var controller = new ReferenceDataController(_referenceDataService);

            var createRequest = _fixture.Build<CreateReferenceDataRequest>()
                .With(r => r.DerivesFrom, (int?)null)
                .Create();

            var createdResponse = _referenceDataService.Create("TestType", createRequest);

            //Act

            var result = controller.GetById("TestType", 1);
            var okResult = result.Result as OkObjectResult;

            //Assert

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
                Assert.That(okResult, Is.Not.Null);
                var response = okResult!.Value as ReferenceDataResponse;
                ResponsesAreEquivalent(createdResponse, response!);
            }
        }

        [Test]
        public void IT_02_Get_ReferenceDataById_ReturnsNotFoundResult_WhenEntityDoesNotExist()
        {
            // Arrange

            var controller = new ReferenceDataController(_referenceDataService);

            //Act

            var result = controller.GetById("TestType", 999);

            //Assert

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public void IT_03_Create_ReferenceData_ReturnsCreatedAtActionResult_WithCreatedResponse_OnSuccess()
        {
            // Arrange

            var controller = new ReferenceDataController(_referenceDataService);

            var createRequest = _fixture.Build<CreateReferenceDataRequest>()
                .With(r => r.DerivesFrom, (int?)null)
                .Create();

            //Act

            var result = controller.Create("TestType", createRequest);
            var createdResult = result.Result as CreatedAtActionResult;

            //Assert

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
                Assert.That(createdResult, Is.Not.Null);
                Assert.That(createdResult!.ActionName, Is.EqualTo(nameof(ReferenceDataController.GetById)));

                var response = createdResult.Value as ReferenceDataResponse;

                Assert.That(response, Is.Not.Null);
                Assert.That(response!.Label, Is.EqualTo(createRequest.Label.Trim()));
                Assert.That(response.ReferenceDataType, Is.EqualTo("TestType"));
            }
        }

        [Test]
        public void IT_04_Create_ReferenceData_ReturnsBadRequestObjectResult_WhenBusinessLogicFails()
        {
            // Arrange

            var controller = new ReferenceDataController(_referenceDataService);

            var createRequest = _fixture.Build<CreateReferenceDataRequest>()
                .With(r => r.DerivesFrom, 999)
                .Create();

            //Act

            var result = controller.Create("TestType", createRequest);
            var badRequestResult = result.Result as BadRequestObjectResult;

            //Assert

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
                Assert.That(badRequestResult, Is.Not.Null);

                var errorResponse = badRequestResult!.Value;
                
                Assert.That(errorResponse, Is.Not.Null);
            }
        }

        [Test]
        public void IT_05_Update_ReferenceData_ReturnsNoContentResult_OnSuccess()
        {
            // Arrange

            var controller = new ReferenceDataController(_referenceDataService);

            var createRequest = _fixture.Build<CreateReferenceDataRequest>()
                .With(r => r.DerivesFrom, (int?)null)
                .Create();

            var created = _referenceDataService.Create("TestType", createRequest);

            var updateRequest = new UpdateReferenceDataRequest
            {
                Label = "UpdatedLabel",
                DerivesFrom = null,
                Active = false
            };

            //Act

            var result = controller.Update("TestType", created.Id, updateRequest);

            //Assert

            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public void IT_06_Update_ReferenceData_ReturnsNotFoundResult_WhenEntityDoesNotExist()
        {
            // Arrange

            var controller = new ReferenceDataController(_referenceDataService);

            var updateRequest = new UpdateReferenceDataRequest
            {
                Label = "UpdatedLabel",
                DerivesFrom = null,
                Active = true
            };

            //Act

            var result = controller.Update("TestType", 999, updateRequest);

            //Assert

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public void IT_07_Update_ReferenceData_ReturnsBadRequestObjectResult_WhenBusinessLogicFails()
        {
            // Arrange

            var controller = new ReferenceDataController(_referenceDataService);

            var createRequest = _fixture.Build<CreateReferenceDataRequest>()
                .With(r => r.DerivesFrom, (int?)null)
                .Create();

            var created = _referenceDataService.Create("TestType", createRequest);

            var updateRequest = new UpdateReferenceDataRequest
            {
                Label = "UpdatedLabel",
                DerivesFrom = 999,
                Active = true
            };

            //Act

            var result = controller.Update("TestType", created.Id, updateRequest);
            var badRequestResult = result as BadRequestObjectResult;

            //Assert

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
                Assert.That(badRequestResult, Is.Not.Null);
            }
        }

        [Test]
        public void IT_08_Delete_ReferenceData_ReturnsNoContentResult_OnSuccess()
        {
            // Arrange

            var controller = new ReferenceDataController(_referenceDataService);

            var createRequest = _fixture.Build<CreateReferenceDataRequest>()
                .With(r => r.DerivesFrom, (int?)null)
                .Create();

            var created = _referenceDataService.Create("TestType", createRequest);

            //Act

            var result = controller.Delete("TestType", created.Id);

            //Assert

            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public void IT_09_Delete_ReferenceData_ReturnsNotFoundResult_WhenEntityDoesNotExist()
        {
            // Arrange

            var controller = new ReferenceDataController(_referenceDataService);

            //Act

            var result = controller.Delete("TestType", 999);

            //Assert

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
}