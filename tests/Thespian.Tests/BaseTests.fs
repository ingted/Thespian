﻿namespace Nessos.Thespian.Tests

open System
open NUnit.Framework
open FsUnit

open Nessos.Thespian
open Nessos.Thespian.Tests.TestDefinitions

[<AbstractClass>]
type BaseTests(primaryProtocolFactory: IPrimaryProtocolFactory) =
  let mutable oldPrimaryProtocolServerFactory = Unchecked.defaultof<IPrimaryProtocolFactory>

  abstract PrimaryProtocolFactory: IPrimaryProtocolFactory
  default __.PrimaryProtocolFactory = primaryProtocolFactory

  [<SetUp>]
  member self.SetUp() =
    oldPrimaryProtocolServerFactory <- Actor.DefaultPrimaryProtocolFactory
    Actor.DefaultPrimaryProtocolFactory <- self.PrimaryProtocolFactory

  [<TearDown>]
  member __.TearDown() = Actor.DefaultPrimaryProtocolFactory <- oldPrimaryProtocolServerFactory
  
  [<Test>]
  member __.``Primitive actor bind - actor name``() =
    let actor = new Actor<TestMessage<unit>>("testActorName", PrimitiveBehaviors.nill)

    actor.Name |> should equal "testActorName"

  [<Test>]
  member __.``Actor.bind primitive behavior - actor name``() =
    let actor = Actor.bind PrimitiveBehaviors.nill

    actor.Name |> should not' (equal String.Empty)


  [<Test>]
  member __.``Unpulished actor, ActorRef.Protocols size is 1``() =
    let actor = Actor.bind PrimitiveBehaviors.nill

    actor.Ref.Protocols.Length |> should equal 1

  [<Test>]
  member __.``ActorRef via property equals ActorRef via operator``() =
    let actor = Actor.bind PrimitiveBehaviors.nill

    let refByProperty = actor.Ref
    let refByOperator = !actor

    refByProperty |> should equal refByOperator

  [<Test>]
  member __.``New actor no pending messages``() =
    let actor = Actor.bind PrimitiveBehaviors.nill

    actor.PendingMessages |> should equal 0

  [<Test>]
  [<ExpectedException(typeof<ArgumentException>)>]
  member __.``Create actor with no protocol``() =
    let actor = new Actor<TestMessage<unit>>("unrealisableActor", Array.empty, PrimitiveBehaviors.nill)
    ()

  [<Test>]
  [<ExpectedException(typeof<ArgumentException>)>]
  member self.``Create actor with non-primitive actor protocol``() =
    let primary = self.PrimaryProtocolFactory.Create<TestMessage<unit>>("unrealisable")
    let primaryRef = new ActorRef<TestMessage<unit>>("unrealisable", [| primary.Client |])
    let tcpProtocol = new Remote.TcpProtocol.Bidirectional.ProtocolServer<TestMessage<unit>>("unrealisable", new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0), primaryRef) :> IProtocolServer<_>
    let actor = new Actor<TestMessage<unit>>("unrealisable", [| tcpProtocol |], PrimitiveBehaviors.nill)
    ()

  [<Test>]
  [<ExpectedException(typeof<ArgumentException>)>]
  member self.``Create actor with name mismatch``() =
    let primary = self.PrimaryProtocolFactory.Create<TestMessage<unit>>("unrealisable") :> IProtocolServer<_>
    let actor = new Actor<TestMessage<unit>>("unrealisable'", [| primary |], PrimitiveBehaviors.nill)
    ()

  [<Test>]
  member __.``Actor.Name = ActorRef.Name after bind``() =
    let actor = Actor.bind PrimitiveBehaviors.nill
    actor.Name |> should equal actor.Ref.Name

  [<Test>]
  member __.``Actor.Name = ActorRef.Id.Name``() =
    let actor = Actor.bind PrimitiveBehaviors.nill
    actor.Name |> should equal actor.Ref.Id.Name
    