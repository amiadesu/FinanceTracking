using Microsoft.AspNetCore.Mvc;
using FinanceTracking.API.Constants;
using FinanceTracking.API.Models;
using FinanceTracking.API.DTOs;
using System;
using System.Linq;

namespace FinanceTracking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    [HttpGet]
    public ActionResult<SystemConfigDto> GetSystemConfiguration()
    {
        var config = new SystemConfigDto
        {
            GroupLimits = new GroupConfigDto
            {
                MaxGroupsPerUser = ServiceConstants.MaxGroupsPerUser,
                MaxMembersPerGroup = ServiceConstants.MaxMembersPerGroup
            },
            ReceiptLimits = new ReceiptConfigDto
            {
                MaxProductsPerReceipt = ServiceConstants.MaxProductsPerReceipt,
                MaxCategoriesPerProduct = ServiceConstants.MaxCategoriesPerProduct
            },
            CategoryRules = new CategoryConfigDto
            {
                DefaultCategoryColor = ServiceConstants.DefaultCategoryColor,
                MaxCategoriesPerGroup = ServiceConstants.MaxCategoriesPerGroup
            },
            
            GroupRoles = Enum.GetValues(typeof(GroupRole))
                             .Cast<GroupRole>()
                             .ToDictionary(r => r.ToString(), r => (int)r)
        };

        return Ok(config);
    }
}