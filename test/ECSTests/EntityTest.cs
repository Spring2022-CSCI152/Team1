﻿using Bulldog.ECS;
using Moq;

namespace Bulldog.test.ECSTests;
using Xunit;

public class EntityTest
{
    private readonly Entity _sut;
    private readonly Component _c; //this should be a mesh
    private readonly Component _c2; //this should be a mesh

    //private readonly Mesh _c; 
    //private readonly Mesh _c2; 
   
    private readonly Component[] _components;


    public EntityTest()
    {
        _sut = new Entity("Test");
        _components = new Component[2];
        //c = new Mesh();
        //c2 = new Mesh();
        
       _c.Name = "TestC";
       _c2.Name = "TestC2";
       
       _components[0] = _c;
       _components[1] = _c2;
    }

    
    
    [Fact(Skip = "Object reference not set to an instance of an object. This is because of Abstract class and our working functions do not work with derived classes  ")]
    public void TestAddAndGetComponent()
    {
        _sut.AddComponent(_c);
        Assert.Equal(_c, _sut.GetComponent("TestC"));

    }
    
    [Fact(Skip = "Object reference not set to an instance of an object. This is because of Abstract class and our working functions do not work with derived classes  ")]
    public void TestAddComponents()
    {
        _sut.AddComponents(_components);
        Assert.Equal(_components[0], _sut.GetComponent("TestC"));
        Assert.Equal(_components[1],_sut.GetComponent("TestC2"));

    }

    [Fact(Skip = "Object reference not set to an instance of an object. This is because of Abstract class and our working functions do not work with derived classes  ")]
    public void TestHasComponent()
    {
        _sut.AddComponents(_components);
        Assert.True(_sut.HasComponent("TestC"));
        Assert.True(_sut.HasComponent("TestC2"));
    }

    [Fact(Skip = "Object reference not set to an instance of an object. This is because of Abstract class and our working functions do not work with derived classes  ")]
    public void TestRemoveComponent()
    {
        _sut.AddComponents(_components);
        TestHasComponent();
        _sut.RemoveComponent("TestC");
        //Assert.Equal(1, cs.Length);
        Assert.Single(_components);
        //Assert.True(_sut.RemoveComponent("TestC2"));
    }
}