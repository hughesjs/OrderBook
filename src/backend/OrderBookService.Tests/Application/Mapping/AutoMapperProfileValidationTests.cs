using System.ComponentModel.Design;
using AutoMapper;

namespace OrderBookService.Tests.Application.Mapping;

public class AutoMapperProfileValidationTests
{
	[Theory]
	[MemberData(nameof(ProfileDataGenerator))]
	public void AllAutoMapperProfilesAreValid(Profile profile)
	{
		MapperConfiguration config = new(c => c.AddProfile(profile));
		config.AssertConfigurationIsValid();
		
	}

	public static IEnumerable<object[]> ProfileDataGenerator => typeof(Program).Assembly.GetTypes()
																			   .Where(t => t.IsAssignableTo(typeof(Profile)) && t != typeof(Profile))
																			   .Select(t => new[] {Activator.CreateInstance(t)!});
}
