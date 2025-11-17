using Moq;
using System;
using System.Collections.Generic;
using Xunit;
using WebAppPrac.Models;
using WebAppPrac.Controllers;
using Microsoft.Extensions.Localization;
using Microsoft.EntityFrameworkCore;
using WebAppPrac.Data;
using Microsoft.AspNetCore.Mvc;

namespace WebSiteXUnits
{
    public class UnitTest1
    {
        [Fact]
        public async Task CreateAccount_UserNotExist_ShouldRegisterAndReturnView()
        {
            // Arrange
            var users = new List<AdminUser>().AsQueryable();
            var mockSet = new Mock<DbSet<AdminUser>>();
            mockSet.As<IQueryable<AdminUser>>().Setup(m => m.Provider).Returns(users.Provider);
            mockSet.As<IQueryable<AdminUser>>().Setup(m => m.Expression).Returns(users.Expression);
            mockSet.As<IQueryable<AdminUser>>().Setup(m => m.ElementType).Returns(users.ElementType);
            mockSet.As<IQueryable<AdminUser>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            var mockContext = new Mock<AppDbContext>();
            mockContext.Setup(c => c.AdminUsers).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            var mockLocalizer = new Mock<IStringLocalizer<AdminController>>();
            mockLocalizer.Setup(l => l["UserIsSuccessfullyRegistered"]).Returns(new LocalizedString("UserIsSuccessfullyRegistered", "Успешно"));

            var controller = new AdminController(mockLocalizer.Object, mockContext.Object);

            // Act
            var result = await controller.CreateAccount("newuser", "pass123") as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("FormForAdmin", result.ViewName);
            Assert.Equal("Успешно", controller.ViewBag.IsRegistered);
        }
    }
}