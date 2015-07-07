namespace UnityFS.Simon
module Agents =
    open UnityFS.Simon.Helpers

//    let piezoEmitter (agents: AgentBus<Message>) =
//        agents.Add("piezo", Agent<Message>.Start(fun inbox ->
//                async {
//                    while true do
//                        let! msg = inbox.Receive()
//                        match msg with
//                        | ActivateColor (c, replyChannel) ->
//                            printfn "Note %A playing..." c
//                            replyChannel.Reply()
//                        | DeactivateColor (c, replyChannel) ->
//                            printfn "Note %A stopped." c        
//                            replyChannel.Reply()
//                        | _ -> ()
//                    }
//            ))

    let cubeHandler (cubes: UnityFS.Simon.GameCubeBase[]) (agents: AgentBus<Message>) =
        agents.Add("cubes", Agent<Message>.Start(fun inbox ->
                async {
                    while true do
                        let! msg = inbox.Receive()
                        match msg with
                        | ActivateColor (c, replyChannel) ->
                            (FindGameCubeByColor cubes c).Activate()
                            replyChannel.Reply()
                        | DeactivateColor (c, replyChannel) ->
                            (FindGameCubeByColor cubes c).Deactivate()
                            replyChannel.Reply()
                        | _ -> ()
                    }
            ))

    let outputHandler (agents: AgentBus<Message>) =
        let cubeBus = agents.TryFind("cubes").Value
        agents.Add("output", Agent<Message>.Start(fun inbox ->
                async {
                    while true do
                        let! msg = inbox.Receive()
                        match msg with
                        | PlayerColor (color, duration, replyChannel) ->
                            cubeBus.PostAndReply(fun reply -> ActivateColor (color, reply))
                            do! Async.Sleep (int duration)
                            cubeBus.PostAndReply(fun reply -> DeactivateColor (color, reply))
                            replyChannel.Reply()
                        | ComputerColors (colors, duration, replyChannel) ->
                            do! Async.Sleep (500)
                            for color in colors do
                                cubeBus.PostAndReply(fun reply -> ActivateColor (color, reply))
                                do! Async.Sleep (int duration)
                                cubeBus.PostAndReply(fun reply -> DeactivateColor (color, reply))
                                do! Async.Sleep (250)
                            replyChannel.Reply()

                        | _ -> ()
                    }
            ))

    let buttonListener (buttonEvt: Event<SimonColor>) (agents: AgentBus<Message>) =
        agents.Add("buttons", Agent<Message>.Start(fun inbox ->
                async {
                    while true do
                        let! msg = inbox.Receive()
                        match msg with
                        | ChooseColor replyChannel ->
                            let! color = Async.AwaitEvent buttonEvt.Publish
                            replyChannel.Reply(color)
                        | _ -> ()
                    }
            ))