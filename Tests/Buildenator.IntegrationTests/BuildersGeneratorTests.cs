using AutoFixture.Xunit2;
using Buildenator.IntegrationTests.Source.Builders;
using FluentAssertions;
using System.Reflection;
using Xunit;

namespace Buildenator.IntegrationTests
{
    public class BuildersGeneratorTests
    {
        [Fact]
        public void BuildersGenerator_HasStaticBulderFactory()
        {
            var builder = EntityBuilder.Entity;

            builder.Should().NotBeNull();
        }

        [Fact]
        public void BuildersGenerator_BuilderNameNotReflectingTargetClassName_ShouldCompile()
        {
            var builder = DifferentNameBuilder.Entity;

            builder.Should().NotBeNull();
        }

        [Theory]
        [AutoData]
        public void BuildersGenerator_OneConstructorWithoutSettableProperties_CreatedWithMethodsByContructorParametersNames(int value, string str)
        {
            var builder = EntityBuilder.Entity;

            var entity = builder.WithPropertyIntGetter(value).WithPropertyStringGetter(str).Build();
            entity.PropertyIntGetter.Should().Be(value);
            entity.PropertyGetter.Should().Be(str);
        }

        [Theory]
        [AutoData]
        public void BuildersGenerator_ZeroConstructorsWithSettableProperties_CreatedWithMethodsBySettablePropertiesNames(int value, string str)
        {
            var builder = SettableEntityWithoutConstructorBuilder.SettableEntityWithoutConstructor;

            var entity = builder.WithPropertyIntGetter(value).WithPropertyGetter(str).Build();
            entity.PropertyIntGetter.Should().Be(value);
            entity.PropertyGetter.Should().Be(str);
        }

        [Theory]
        [AutoData]
        public void BuildersGenerator_OneConstructorWithSettableProperties_CreatedWithMethodsBySettablePropertiesNamesIfInBothPlaces(
            int value, string str, string[] strs, int[] arr)
        {
            var builder = SettableEntityWithConstructorBuilder.SettableEntityWithConstructor;

            var entity = builder.WithPropertyInt(value).WithProperty(str)
                .WithNoConstructorProperty(strs).WithPrivateField(arr).Build();
            entity.PropertyInt.Should().Be(value);
            entity.Property.Should().Be(str);
            entity.NoConstructorProperty.Should().BeSameAs(strs);
            entity.GetType()
            .GetField("_privateField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(entity).Should().BeSameAs(arr);
        }
    }
}