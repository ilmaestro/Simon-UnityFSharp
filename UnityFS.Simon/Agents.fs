namespace UnityFS.Simon
module Agents =
    open UnityFS.Simon.Helpers

    let piezoEmitter (agents: AgentBus<Message>) =
        agents.Add("piezo", Agent<Message>.Start(fun inbox ->
                async {
                    while true do
                        let! msg = inbox.Receive()
                        match msg with
                        | ActivateColor (c, replyChannel) ->
                            printfn "Note %A playing..." c
                            replyChannel.Reply()
                        | DeactivateColor (c, replyChannel) ->
                            printfn "Note %A stopped." c        
                            replyChannel.Reply()
                        | _ -> ()
                    }
            ))

    let ledEmitter (cubes: UnityFS.Simon.GameCubeBase[]) (agents: AgentBus<Message>) =
        agents.Add("led", Agent<Message>.Start(fun inbox ->
                async {
                    while true do
                        let! msg = inbox.Receive()
                        match msg with
                        | ActivateColor (c, replyChannel) ->
                            (FindGameCubeByColor cubes c).Activate()
                            printfn "turning %A LED on" c
                            replyChannel.Reply()
                        | DeactivateColor (c, replyChannel) ->
                            (FindGameCubeByColor cubes c).Deactivate()
                            printfn "turning %A LED off" c    
                            replyChannel.Reply()
                        | _ -> ()
                    }
            ))

    let outputHandler (agents: AgentBus<Message>) =
        let piezoBus = agents.TryFind("piezo")
        let ledBus = agents.TryFind("led")
        agents.Add("output", Agent<Message>.Start(fun inbox ->
                async {
                    while true do
                        let! msg = inbox.Receive()
                        match msg with
                        | PlayerColor (color, duration, replyChannel) ->
                            match piezoBus with
                            | Some piezo -> piezo.PostAndReply(fun reply -> ActivateColor (color, reply))
                            | None -> printfn "Piezo inactive"

                            match ledBus with
                            | Some led -> led.PostAndReply(fun reply -> ActivateColor (color, reply))
                            | None -> printfn "LED inactive"

                            do! Async.Sleep (int duration)

                            if piezoBus.IsSome then piezoBus.Value.PostAndReply(fun reply -> DeactivateColor (color, reply))
                            if ledBus.IsSome then ledBus.Value.PostAndReply(fun reply -> DeactivateColor (color, reply))
                            
                            replyChannel.Reply()
                        | ComputerColor (color, duration, replyChannel) ->
                            match piezoBus with
                            | Some piezo -> piezo.PostAndReply(fun reply -> ActivateColor (color, reply))
                            | None -> printfn "Piezo inactive"

                            match ledBus with
                            | Some led -> led.PostAndReply(fun reply -> ActivateColor (color, reply))
                            | None -> printfn "LED inactive"

                            do! Async.Sleep (int duration)

                            if piezoBus.IsSome then piezoBus.Value.PostAndReply(fun reply -> DeactivateColor (color, reply))
                            if ledBus.IsSome then ledBus.Value.PostAndReply(fun reply -> DeactivateColor (color, reply))
                            
                            do! Async.Sleep (250)
                            replyChannel.Reply()

                        | _ -> ()
                    }
            ))

    let buttonListener (buttonEvt: Event<UnityFS.Simon.SimonColor>) (agents: AgentBus<Message>) =
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