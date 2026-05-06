using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ReferenceDataApi.Controllers;
using ReferenceDataApi.Dtos;
using ReferenceDataApi.Services;

namespace ReferenceDataApi.UnitTests
{
    public class ReferenceDataControllerTests
    {
        private Mock<IReferenceDataService> _referenceDataServiceStub;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _referenceDataServiceStub = new Mock<IReferenceDataService>();
            _fixture = new Fixture();
        }

        [Test]
        public void UT_01_Get_ReferenceDataById_ReturnsOkObjectResult_WithExpectedResponse_WhenEntityExists()
        {
            // Arrange

            var expectedResponse = _fixture.Create<ReferenceDataResponse>();

            _referenceDataServiceStub
                .Setup(s => s.Get(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(expectedResponse);

            var controller = new ReferenceDataController(_referenceDataServiceStub.Object);

            //Act

            var result = controller.GetById("TestType", 1);
            var okResult = result.Result as OkObjectResult;

            //Assert

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
                Assert.That(okResult, Is.Not.Null);
                Assert.That(okResult!.Value, Is.EqualTo(expectedResponse));
            }
        }

        [Test]
        public void UT_02_Get_ReferenceDataById_ReturnsNotFoundResult_WhenEntityDoesNotExist()
        {
            // Arrange

            _referenceDataServiceStub
                .Setup(s => s.Get(It.IsAny<string>(), It.IsAny<int>()))
                .Returns((ReferenceDataResponse?)null);

            var controller = new ReferenceDataController(_referenceDataServiceStub.Object);

            // Act

            var result = controller.GetById("TestType", 1);

            // Assert

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public void UT_03_Create_ReturnsCreatedAtActionResult_WhenCreateSucceeds()
        {
            // Arrange

            var expectedResponse = _fixture.Create<ReferenceDataResponse>();
            var request = _fixture.Create<CreateReferenceDataRequest>();

            _referenceDataServiceStub
                .Setup(s => s.Create(It.IsAny<string>(), It.IsAny<CreateReferenceDataRequest>()))
                .Returns(expectedResponse);

            var controller = new ReferenceDataController(_referenceDataServiceStub.Object);

            // Act

            var result = controller.Create("TestType", request);
            var createdResult = result.Result as CreatedAtActionResult;

            // Assert

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
                Assert.That(createdResult, Is.Not.Null);
                Assert.That(createdResult!.Value, Is.EqualTo(expectedResponse));
            }
        }

        [Test]
        public void UT_04_Create_ReturnsBadRequestResult_WhenInvalidOperationExceptionIsThrown()
        {
            // Arrange

            var request = _fixture.Create<CreateReferenceDataRequest>();

            _referenceDataServiceStub
                .Setup(s => s.Create(It.IsAny<string>(), It.IsAny<CreateReferenceDataRequest>()))
                .Throws<InvalidOperationException>();

            var controller = new ReferenceDataController(_referenceDataServiceStub.Object);

            // Act

            var result = controller.Create("TestType", request);

            // Assert

            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public void UT_05_Create_ReturnsBadRequestResult_WhenArgumentExceptionIsThrown()
        {
            // Arrange

            var request = _fixture.Create<CreateReferenceDataRequest>();

            _referenceDataServiceStub
                .Setup(s => s.Create(It.IsAny<string>(), It.IsAny<CreateReferenceDataRequest>()))
                .Throws<ArgumentException>();

            var controller = new ReferenceDataController(_referenceDataServiceStub.Object);

            // Act

            var result = controller.Create("TestType", request);

            // Assert

            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public void UT_06_Update_ReturnsNoContentResult_WhenUpdateSucceeds()
        {
            // Arrange

            var expectedResponse = _fixture.Create<ReferenceDataResponse>();
            var request = _fixture.Create<UpdateReferenceDataRequest>();
            string? error = null;

            _referenceDataServiceStub
                .Setup(s => s.Update(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<UpdateReferenceDataRequest>(), out error))
                .Returns(true);

            _referenceDataServiceStub
                .Setup(s => s.Get(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(expectedResponse);

            var controller = new ReferenceDataController(_referenceDataServiceStub.Object);

            // Act

            var result = controller.Update("TestType", 1, request);
            var noContentResult = result as NoContentResult;

            // Assert

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.InstanceOf<NoContentResult>());
                Assert.That(noContentResult, Is.Not.Null);
            }
        }

        [Test]
        public void UT_07_Update_ReturnsNotFoundResult_WhenEntityDoesNotExist()
        {
            // Arrange

            var request = _fixture.Create<UpdateReferenceDataRequest>();
            string? error = null;

            _referenceDataServiceStub
                .Setup(s => s.Update(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<UpdateReferenceDataRequest>(), out error))
                .Returns(false);

            var controller = new ReferenceDataController(_referenceDataServiceStub.Object);

            // Act

            var result = controller.Update("TestType", 1, request);

            // Assert

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public void UT_08_Update_ReturnsBadRequestResult_WhenServiceReturnsAnError()
        {
            // Arrange

            var request = _fixture.Create<UpdateReferenceDataRequest>();
            string? error = "DerivesFrom parent not found.";

            _referenceDataServiceStub
                .Setup(s => s.Update(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<UpdateReferenceDataRequest>(), out error))
                .Returns(false);

            var controller = new ReferenceDataController(_referenceDataServiceStub.Object);

            // Act

            var result = controller.Update("TestType", 1, request);
            var badRequestResult = result as BadRequestObjectResult;

            // Assert

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
                Assert.That(badRequestResult, Is.Not.Null);
                Assert.That(badRequestResult!.Value, Is.Not.Null);
                var errorProperty = badRequestResult.Value!.GetType().GetProperty("error");
                Assert.That(errorProperty, Is.Not.Null);
                Assert.That(errorProperty!.GetValue(badRequestResult.Value), Is.EqualTo(error));
            }
        }

        [Test]
        public void UT_09_Update_ReturnsBadRequestResult_WhenArgumentExceptionIsThrown()
        {
            // Arrange

            var request = _fixture.Create<UpdateReferenceDataRequest>();

            _referenceDataServiceStub
                .Setup(s => s.Update(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<UpdateReferenceDataRequest>(), out It.Ref<string?>.IsAny))
                .Throws<ArgumentException>();

            var controller = new ReferenceDataController(_referenceDataServiceStub.Object);

            // Act

            var result = controller.Update("TestType", 1, request);

            // Assert

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public void UT_10_Delete_ReturnsNoContent_WhenDeleteSucceeds()
        {
            // Arrange

            _referenceDataServiceStub
                .Setup(s => s.Delete(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(true);

            var controller = new ReferenceDataController(_referenceDataServiceStub.Object);

            // Act

            var result = controller.Delete("TestType", 1);

            // Assert

            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public void UT_11_Delete_ReturnsNotFound_WhenEntityDoesNotExist()
        {
            // Arrange

            _referenceDataServiceStub
                .Setup(s => s.Delete(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(false);

            var controller = new ReferenceDataController(_referenceDataServiceStub.Object);

            // Act

            var result = controller.Delete("TestType", 1);

            // Assert

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public void UT_12_Delete_ReturnsBadRequestResult_WhenArgumentExceptionIsThrown()
        {
            // Arrange

            _referenceDataServiceStub
                .Setup(s => s.Delete(It.IsAny<string>(), It.IsAny<int>()))
                .Throws<ArgumentException>();

            var controller = new ReferenceDataController(_referenceDataServiceStub.Object);

            // Act

            var result = controller.Delete("TestType", 1);

            // Assert

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }
    }
}