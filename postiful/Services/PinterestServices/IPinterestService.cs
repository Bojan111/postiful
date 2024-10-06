using System;
using postiful.Models.PinterestModels;
using Microsoft.EntityFrameworkCore.Metadata;
using OpenQA.Selenium;

namespace EnginaCode.Services.PinterestServices
{
	public interface IPinterestService
    {
        IWebDriver CreatePin(CreatePinterestPin pinModel);
    }
}