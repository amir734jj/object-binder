using System;
using AutoFixture;
using core.Builders;
using core.Tests.Models;
using Xunit;

namespace core.Tests
{
    public class ObjectBinderTest
    {
        private readonly Fixture _fixture;
        
        private readonly Type _boundType;

        public ObjectBinderTest()
        {
            _fixture = new Fixture();

            _boundType = ObjectBinderBuilder.New<Entity, ICommon>()
                .SourceRef(x => x.SourceRef)
                .Bind(x => x.Name, x => x.NameC)
                .Bind(x => x.Alive, x => x.AliveC)
                .Build()
                .BoundType;
        }

        [Fact]
        public void Test__DefaultValues()
        {
            // Arrange
            var source = _fixture.Create<Entity>();
            var proxy = (ICommon) Activator.CreateInstance(_boundType, source);

            // Act, Assert
            Assert.Equal(source.Name, proxy.NameC);
            Assert.Equal(source.Alive, proxy.AliveC);
            Assert.Equal(source, proxy.SourceRef);
        }
        
        [Fact]
        public void Test__ValueChange()
        {
            // Arrange
            var source = _fixture.Create<Entity>();
            var proxy = (ICommon) Activator.CreateInstance(_boundType, source);
            
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
    }
}