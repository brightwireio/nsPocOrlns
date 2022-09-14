﻿global using System;
global using System.Linq;
global using System.Threading.Tasks;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Azure.WebJobs;
global using Microsoft.Azure.WebJobs.Extensions.Http;
global using Microsoft.AspNetCore.Http;
global using Microsoft.Extensions.Logging;
global using Newtonsoft.Json;
global using System.Threading;
global using Microsoft.Azure.WebJobs.Extensions.DurableTask;
global using System.Collections.Generic;
global using nsPocOrlns.Common;
global using nsPocOrlns.Infrastructure.Repositories;
global using nsPocOrlns.Infrastructure.EventStreaming;
global using Microsoft.Azure.Functions.Extensions.DependencyInjection;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using nsPocOrlns.Infrastructure.Persistence;
global using nsPocOrlns.TrafficSimulator;
