using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinanceTracking.API.Data;
using FinanceTracking.API.Models;
using FinanceTracking.API.DTOs;

namespace FinanceTracking.API.Services;

public class GroupMemberService
{
    private readonly FinanceDbContext _dbContext;

    public GroupMemberService(FinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }
}