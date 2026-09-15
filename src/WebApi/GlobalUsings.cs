global using Microsoft.EntityFrameworkCore;
global using static System.ArgumentException;
global using static System.ArgumentNullException;
global using static System.ArgumentOutOfRangeException;
global using Ardalis.Specification;
global using FluentValidation;
global using ErrorOr;
global using Vogen;
global using SSW.VerticalSliceArchitecture.Common.Persistence;

// HotChocolate's implicit usings bring in HotChocolate.Error, which collides with the ErrorOr
// type the domain returns. "Error" means the domain's result type throughout this project.
global using Error = ErrorOr.Error;
