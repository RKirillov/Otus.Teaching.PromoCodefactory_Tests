﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Otus.Teaching.PromoCodeFactory.Core.Abstractions.Repositories;
using Otus.Teaching.PromoCodeFactory.Core.Domain.PromoCodeManagement;
using Otus.Teaching.PromoCodeFactory.WebHost.Controllers;
using Otus.Teaching.PromoCodeFactory.WebHost.Models;
using Xunit;
using YamlDotNet.Core;


namespace Otus.Teaching.PromoCodeFactory.UnitTests.WebHost.Controllers.Partners
{
    public class CancelPartnerPromoCodeLimitAsyncTests
    {
        private readonly Mock<IRepository<Partner>> _partnersRepositoryMock;
        private readonly PartnersController _partnersController;
        private readonly DateTime specificDateTime = new DateTime(2021, 5, 1, 21, 53, 30);
        //private readonly SetPartnerPromoCodeLimitRequest _partnerLimitRequest;
        //private readonly Mock<SetPartnerPromoCodeLimitRequest> _setPartnerPromoCodeLimitRequest;

        public CancelPartnerPromoCodeLimitAsyncTests()
        {
            //TODO не работает
            //Func<DateTime> getDateTime = () => DateTime.Now;

            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            //fixture.Inject(specificDateTime);
            fixture.Register<DateTime?>(() => specificDateTime);
            //This means that every time we request an instance of a frozen type, we will get the same instance. You can think of it as registering a singleton instance in an IoC container.
            _partnersRepositoryMock = fixture.Freeze<Mock<IRepository<Partner>>>();
            //Before creating the object, the Build method can be used to add one-time customizations to be used for the creation of the next variable.
            _partnersController = fixture.Build<PartnersController>()
                //The With construct allows the customization of writeable properties and public fields.
                .With(x => x.CurrentDateTime, () => specificDateTime)
                .OmitAutoProperties()
                .Create();
            //fixture.Register <DateTime>(() => specificDateTime);
            //var someEntity = new Fixture().Build<Entity>().With(e => e.Name, "Important For Test").Without(e => e.Group).Create();
            //_partnerLimitRequest = fixture.Build<SetPartnerPromoCodeLimitRequest>()
            //    .With(f => f.Limit, 10)
            //    .With(f => f.EndDate, DateTime.Now).Create();

        }

        public Partner CreateBasePartner(string id = "def47943-7aaf-44a1-ae21-05aa4948b165", DateTime? cancelDate = null)
        {
            var partner = new Partner()
            {
                Id = Guid.Parse(id),
                Name = "Суперигрушки",
                IsActive = true,
                PartnerLimits = new List<PartnerPromoCodeLimit>()
                {
                    new PartnerPromoCodeLimit()
                    {
                        Id = Guid.Parse("e00633a5-978a-420e-a7d6-3e1dab116393"),
                        CreateDate = new DateTime(2020, 07, 9),
                        EndDate = new DateTime(2020, 10, 9),
                        Limit = 100,
                        CancelDate= cancelDate
                    }
                }
            };

            return partner;
        }

        [Fact]
        public async void CancelPartnerPromoCodeLimit_PartnerIsNotFound_ReturnsNotFound()
        {
            // Arrange
            var partnerId = Guid.Parse("def47943-7aaf-44a1-ae21-05aa4948b165");
            Partner partner = null;

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);

            // Act
            var result = await _partnersController.CancelPartnerPromoCodeLimitAsync(partnerId);

            // Assert
            result.Should().BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async void CancelPartnerPromoCodeLimit_PartnerIsNotActive_ReturnsBadRequest()
        {
            // Arrange без разницы какой id устанавливать
            var partnerId = Guid.Parse("7d994823-8226-4273-b063-1a95f3cc1df8");
            //var partnerId=Guid.Empty;
            var partner = CreateBasePartner("7d994823-8226-4273-b063-1a95f3cc1df8");
            partner.IsActive = false;

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);

            // Act
            var result = await _partnersController.CancelPartnerPromoCodeLimitAsync(partnerId);

            // Assert
            result.Should().BeAssignableTo<BadRequestObjectResult>();
        }
    }
}