using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ReferenceDataApi.Controllers;
using ReferenceDataApi.Dtos;
using ReferenceDataApi.Repositories;
using ReferenceDataApi.Services;

namespace ReferenceDataApi.IntegrationTests
{
    public class ReferenceDataServiceTests
    {
        private InMemoryReferenceDataRepository _referenceDataRepository;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _referenceDataRepository = new InMemoryReferenceDataRepository();
            _fixture = new Fixture();
        }

        [Test]
        public void IT__10_Create_TrimsLabel_BeforeAddingEntity_ToRepository()
        {
            // Arrange

            var service = new ReferenceDataService(_referenceDataRepository);

            var createRequest = _fixture.Build<CreateReferenceDataRequest>()
                .With(r => r.DerivesFrom, (int?)null)
                .With(r => r.Label, "  Test Label With Spaces  ")
                .Create();

            var createdResponse = service.Create("TestType", createRequest);

            //Act

            var result = service.Get("TestType", 1);

            //Assert

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.Label, Is.EqualTo(result.Label.Trim()));
            }
        }

        [Test]
        public void IT_11_Create_AutoGeneratesCreatedDate()
        {
            // Arrange

            var service = new ReferenceDataService(_referenceDataRepository);

            var createRequest = _fixture.Build<CreateReferenceDataRequest>()
                .With(r => r.DerivesFrom, (int?)null)
                .Create();

            // Act

            var result = service.Create("TestType", createRequest);

            // Assert

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.CreatedDate, Is.Not.Default);
            }
        }

        [Test]
        public void IT_12_Create_UsesAutoAssignedIds()
        {
            // Arrange

            var service = new ReferenceDataService(_referenceDataRepository);

            var createRequest = _fixture.Build<CreateReferenceDataRequest>()
                .With(r => r.DerivesFrom, (int?)null)
                .Create();

            // Act

            var firstResult = service.Create("TestType", createRequest);
            var secondResult = service.Create("TestType", createRequest);

            // Assert

            using (Assert.EnterMultipleScope())
            {
                Assert.That(firstResult.Id, Is.GreaterThan(0));
                Assert.That(secondResult.Id, Is.GreaterThan(0));
                Assert.That(secondResult.Id, Is.Not.EqualTo(firstResult.Id));
            }
        }

        [Test]
        public void IT_13_Update_PreservesCreatedDate()
        {
            // Arrange

            var service = new ReferenceDataService(_referenceDataRepository);

            var createRequest = _fixture.Build<CreateReferenceDataRequest>()
                .With(r => r.DerivesFrom, (int?)null)
                .Create();

            var createdResponse = service.Create("TestType", createRequest);
            var createdBeforeUpdate = service.Get("TestType", createdResponse.Id);

            var updateRequest = new UpdateReferenceDataRequest
            {
                Label = "Updated Label",
                DerivesFrom = null,
                Active = false
            };

            // Act

            var updateResult = service.Update("TestType", createdResponse.Id, updateRequest, out var error);
            var createdAfterUpdate = service.Get("TestType", createdResponse.Id);

            // Assert

            using (Assert.EnterMultipleScope())
            {
                Assert.That(updateResult, Is.True);
                Assert.That(error, Is.Null);
                Assert.That(createdBeforeUpdate, Is.Not.Null);
                Assert.That(createdAfterUpdate, Is.Not.Null);
                Assert.That(createdAfterUpdate!.CreatedDate, Is.EqualTo(createdBeforeUpdate!.CreatedDate));
            }
        }

        [Test]
        public void IT_14_Update_RejectsSelfReferencingDerivesFromProperty()
        {
            // Arrange

            var service = new ReferenceDataService(_referenceDataRepository);

            var createRequest = _fixture.Build<CreateReferenceDataRequest>()
                .With(r => r.DerivesFrom, (int?)null)
                .Create();

            var createdResponse = service.Create("TestType", createRequest);

            var updateRequest = new UpdateReferenceDataRequest
            {
                Label = "Updated Label",
                DerivesFrom = createdResponse.Id,
                Active = true
            };

            // Act

            var result = service.Update("TestType", createdResponse.Id, updateRequest, out var error);

            // Assert

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(error, Is.Not.Null.And.Not.Empty);
            }
        }

        [Test]
        public void IT_15_Update_RejectsNonExistentDerivesFromId()
        {
            // Arrange

            var service = new ReferenceDataService(_referenceDataRepository);

            var createRequest = _fixture.Build<CreateReferenceDataRequest>()
                .With(r => r.DerivesFrom, (int?)null)
                .Create();

            var createdResponse = service.Create("TestType", createRequest);

            var updateRequest = new UpdateReferenceDataRequest
            {
                Label = "Updated Label",
                DerivesFrom = 999,
                Active = true
            };

            // Act

            var result = service.Update("TestType", createdResponse.Id, updateRequest, out var error);

            // Assert

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(error, Is.Not.Null.And.Not.Empty);
            }
        }
    }
}