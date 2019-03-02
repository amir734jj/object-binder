using System;
using AutoFixture;
using core.Builders;
using core.Tests.Models;
using LamarCompiler;
using Xunit;

namespace core.Tests
{
    public class ObjectBinderTest
    {
        private readonly Fixture _fixture;
        
        private readonly AssemblyGenerator _assemblyGenerator;

        public ObjectBinderTest()
        {
            _fixture = new Fixture();
            _assemblyGenerator = new AssemblyGenerator();
        }

        [Fact]
        public void Test__Interface_DefaultValues()
        {
            // Arrange
            var boundType = ObjectBinderBuilder.New<Entity, ICommon>()
                .Bind(x => x.Name, x => x.NameC)
                .Bind(x => x.Alive, x => x.AliveC)
                .WithAssemblyGenerator(_assemblyGenerator)
                .Build()
                .BoundType;
            
            var source = _fixture.Create<Entity>();
            var proxy = (ICommon) Activator.CreateInstance(boundType, source);

            // Act, Assert
            Assert.Equal(source.Name, proxy.NameC);
            Assert.Equal(source.Alive, proxy.AliveC);
        }
        
        [Fact]
        public void Test__Interface_ValueChange()
        {
            // Arrange
            var boundType = ObjectBinderBuilder.New<Entity, ICommon>()
                .Bind(x => x.Name, x => x.NameC)
                .Bind(x => x.Alive, x => x.AliveC)
                .WithAssemblyGenerator(_assemblyGenerator)
                .Build()
                .BoundType;

            var source = _fixture.Create<Entity>();
            var proxy = (ICommon) Activator.CreateInstance(boundType, source);
            
            var (updatedName, updatedAlive) = (_fixture.Create<string>(), _fixture.Create<bool>());
            
            // Act
            proxy.NameC = updatedName;
            proxy.AliveC = updatedAlive;
            
            // Assert
            Assert.Equal(updatedName, source.Name);
            Assert.Equal(updatedAlive, source.Alive);
            Assert.Equal(source.Name, proxy.NameC);
            Assert.Equal(source.Alive, proxy.AliveC);
        }
        
        [Fact]
        public void Test_Class_DefaultValues()
        {
            // Arrange
            var boundType = ObjectBinderBuilder.New<Entity, Common>()
                .Bind(x => x.Name, x => x.NameC)
                .Bind(x => x.Alive, x => x.AliveC)
                .WithAssemblyGenerator(_assemblyGenerator)
                .Build()
                .BoundType;
            
            var source = _fixture.Create<Entity>();
            var proxy = (Common) Activator.CreateInstance(boundType, source);
            
            // Act, Assert
            Assert.Equal(source.Name, proxy.NameC);
            Assert.Equal(source.Alive, proxy.AliveC);
        }
        
        [Fact]
        public void Test__Class_ValueChange()
        {
            // Arrange
            var boundType = ObjectBinderBuilder.New<Entity, Common>()
                .Bind(x => x.Name, x => x.NameC)
                .Bind(x => x.Alive, x => x.AliveC)
                .WithAssemblyGenerator(_assemblyGenerator)
                .Build()
                .BoundType;

            var source = _fixture.Create<Entity>();
            var proxy = (Common) Activator.CreateInstance(boundType, source);
            
            var extraProperty = _fixture.Create<string>();
            var (updatedName, updatedAlive) = (_fixture.Create<string>(), _fixture.Create<bool>());

            // Act
            proxy.NameC = updatedName;
            proxy.AliveC = updatedAlive;
            proxy.ExtraProperty = extraProperty;
            
            // Assert
            Assert.Equal(updatedName, source.Name);
            Assert.Equal(updatedAlive, source.Alive);
            Assert.Equal(source.Name, proxy.NameC);
            Assert.Equal(source.Alive, proxy.AliveC);
            Assert.Equal(extraProperty, proxy.ExtraProperty);
        }
    }
}