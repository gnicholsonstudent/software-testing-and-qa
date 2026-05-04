using AutoFixture;
using Moq;
using ReferenceDataApi.Domain;
using ReferenceDataApi.Dtos;
using ReferenceDataApi.Repositories;
using ReferenceDataApi.Services;

namespace ReferenceDataApi.UnitTests
{
    public class ReferenceDataServiceTests
    {
        private Mock<IReferenceDataRepository> _referenceDataRepositoryStub;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _referenceDataRepositoryStub = new Mock<IReferenceDataRepository>();
            _fixture = new Fixture();
        }

        [Test]
        public void UT_12_Get_ReturnsReferenceDataResponse_WhenEntityExists()
        {
            // Arrange

            var expectedItem = _fixture.Create<ReferenceDataItem>();

            _referenceDataRepositoryStub
                .Setup(r => r.TryGet(It.IsAny<string>(), It.IsAny<int>(), out expectedItem))
                .Returns(true);

            var service = new ReferenceDataService(_referenceDataRepositoryStub.Object);

            // Act

            var result = service.Get("TestType", 1);

            // Assert

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.ReferenceDataType, Is.EqualTo("TestType"));
                Assert.That(result.Id, Is.EqualTo(expectedItem.Id));
                Assert.That(result.Label, Is.EqualTo(expectedItem.Label));
                Assert.That(result.DerivesFrom, Is.EqualTo(expectedItem.DerivesFrom));
                Assert.That(result.Active, Is.EqualTo(expectedItem.Active));
                Assert.That(result.CreatedDate, Is.EqualTo(expectedItem.CreatedDate));
            }
        }

        [Test]
        public void UT_13_Get_ReturnsNull_WhenEntityDoesNotExist()
        {
            // Arrange

            ReferenceDataItem? item = null;

            _referenceDataRepositoryStub
                .Setup(r => r.TryGet(It.IsAny<string>(), It.IsAny<int>(), out item))
                .Returns(false);

            var service = new ReferenceDataService(_referenceDataRepositoryStub.Object);

            // Act

            var result = service.Get("TestType", 1);

            // Assert

            Assert.That(result, Is.Null);
        }

        [Test]
        public void UT_14_Create_ReturnsReferenceDataResponse_WhenRequestIsValid()
        {
            // Arrange

            var request = _fixture.Build<CreateReferenceDataRequest>()
                .With(r => r.DerivesFrom, (int?)null)
                .Create();
            var createdItem = _fixture.Build<ReferenceDataItem>()
                .With(i => i.Label, request.Label.Trim())
                .With(i => i.DerivesFrom, request.DerivesFrom)
                .With(i => i.Active, request.Active)
                .Create();

            _referenceDataRepositoryStub
                .Setup(r => r.Add(It.IsAny<string>(), It.IsAny<ReferenceDataItem>()))
                .Returns(createdItem);

            var service = new ReferenceDataService(_referenceDataRepositoryStub.Object);

            // Act

            var result = service.Create("TestType", request);

            // Assert

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.ReferenceDataType, Is.EqualTo("TestType"));
                Assert.That(result.Id, Is.EqualTo(createdItem.Id));
                Assert.That(result.Label, Is.EqualTo(createdItem.Label));
                Assert.That(result.DerivesFrom, Is.EqualTo(createdItem.DerivesFrom));
                Assert.That(result.Active, Is.EqualTo(createdItem.Active));
                Assert.That(result.CreatedDate, Is.EqualTo(createdItem.CreatedDate));
            }
        }

        [Test]
        public void UT_15_Create_ThrowsArgumentException_WhenTypeIsInvalid()
        {
            // Arrange

            var request = _fixture.Create<CreateReferenceDataRequest>();
            var service = new ReferenceDataService(_referenceDataRepositoryStub.Object);

            // Act

            var act = () => service.Create("", request);

            // Assert
            Assert.That(act, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void UT_16_Create_ThrowsInvalidOperationException_WhenDerivesFromIsInvalid()
        {
            // Arrange

            var request = _fixture.Build<CreateReferenceDataRequest>()
                .With(r => r.DerivesFrom, 99)
                .Create();

            _referenceDataRepositoryStub
                .Setup(r => r.Exists(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(false);

            var service = new ReferenceDataService(_referenceDataRepositoryStub.Object);

            // Act
            var act = () => service.Create("TestType", request);

            // Assert

            Assert.That(act, Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void UT_17_Update_ReturnsTrueAndNullError_WhenUpdateSucceeds()
        {
            // Arrange

            var request = _fixture.Build<UpdateReferenceDataRequest>()
                .With(r => r.DerivesFrom, (int?)null)
                .Create();
            var existingItem = _fixture.Build<ReferenceDataItem>()
                .With(i => i.Id, 1)
                .Create();

            _referenceDataRepositoryStub
                .Setup(r => r.TryGet(It.IsAny<string>(), It.IsAny<int>(), out existingItem))
                .Returns(true);

            _referenceDataRepositoryStub
                .Setup(r => r.Update(It.IsAny<string>(), It.IsAny<ReferenceDataItem>()))
                .Returns(true);

            var service = new ReferenceDataService(_referenceDataRepositoryStub.Object);

            // Act

            var result = service.Update("TestType", 1, request, out var error);

            // Assert

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(error, Is.Null);
            }
        }

        [Test]
        public void UT_18_Update_ReturnsFalseAndNullError_WhenEntityDoesNotExist()
        {
            // Arrange

            var request = _fixture.Create<UpdateReferenceDataRequest>();
            ReferenceDataItem? existingItem = null;

            _referenceDataRepositoryStub
                .Setup(r => r.TryGet(It.IsAny<string>(), It.IsAny<int>(), out existingItem))
                .Returns(false);

            var service = new ReferenceDataService(_referenceDataRepositoryStub.Object);

            // Act

            var result = service.Update("TestType", 1, request, out var error);

            // Assert

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(error, Is.Null);
            }
        }

        [Test]
        public void UT_19_Update_ReturnsFalseAndError_WhenDerivesFromReferencesItself()
        {
            // Arrange

            var request = _fixture.Build<UpdateReferenceDataRequest>()
                .With(r => r.DerivesFrom, 1)
                .Create();
            var existingItem = _fixture.Build<ReferenceDataItem>()
                .With(i => i.Id, 1)
                .Create();

            _referenceDataRepositoryStub
                .Setup(r => r.TryGet(It.IsAny<string>(), It.IsAny<int>(), out existingItem))
                .Returns(true);

            var service = new ReferenceDataService(_referenceDataRepositoryStub.Object);

            // Act

            var result = service.Update("TestType", 1, request, out var error);

            // Assert

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(error, Is.Not.Null.And.Not.Empty);
            }
        }

        [Test]
        public void UT_20_Update_ReturnsFalseAndError_WhenParentDoesNotExist()
        {
            // Arrange

            var request = _fixture.Build<UpdateReferenceDataRequest>()
                .With(r => r.DerivesFrom, 99)
                .Create();
            var existingItem = _fixture.Build<ReferenceDataItem>()
                .With(i => i.Id, 1)
                .Create();

            _referenceDataRepositoryStub
                .Setup(r => r.TryGet(It.IsAny<string>(), It.IsAny<int>(), out existingItem))
                .Returns(true);

            _referenceDataRepositoryStub
                .Setup(r => r.Exists(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(false);

            var service = new ReferenceDataService(_referenceDataRepositoryStub.Object);

            // Act

            var result = service.Update("TestType", 1, request, out var error);

            // Assert

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(error, Is.Not.Null.And.Not.Empty);
            }
        }

        [Test]
        public void UT_21_Update_ThrowsArgumentException_WhenTypeIsInvalid()
        {
            // Arrange

            var request = _fixture.Build<UpdateReferenceDataRequest>()
                .With(r => r.DerivesFrom, 1)
                .Create();
            var service = new ReferenceDataService(_referenceDataRepositoryStub.Object);

            // Act

            var act = () => service.Update("", 1, request, out var error);

            /// Assert

            Assert.That(act, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void UT_22_Delete_ReturnsTrue_WhenDeleteSucceeds()
        {
            // Arrange

            _referenceDataRepositoryStub
                .Setup(r => r.Delete(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(true);

            var service = new ReferenceDataService(_referenceDataRepositoryStub.Object);

            // Act

            var result = service.Delete("TestType", 1);

            // Assert

            Assert.That(result, Is.True);
        }

        [Test]
        public void UT_23_Delete_ReturnsFalse_WhenEntityDoesNotExist()
        {
            // Arrange

            _referenceDataRepositoryStub
                .Setup(r => r.Delete(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(false);

            var service = new ReferenceDataService(_referenceDataRepositoryStub.Object);

            // Act

            var result = service.Delete("TestType", 1);

            // Assert

            Assert.That(result, Is.False);
        }

        [Test]
        public void UT_24_Delete_ThrowsArgumentException_WhenTypeIsInvalid()
        {
            // Arrange

            var service = new ReferenceDataService(_referenceDataRepositoryStub.Object);

            // Act 

            var act = () => service.Delete("", 1);

            // Assert

            Assert.That(act, Throws.TypeOf<ArgumentException>());
        }
    }
}