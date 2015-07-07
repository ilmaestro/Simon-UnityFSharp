namespace UnityFS.Simon
module Helpers =
    open UnityFS.Simon

    let intToColor int =
        match int with
        | 1 -> UnityFS.Simon.SimonColor.Red
        | 2 -> UnityFS.Simon.SimonColor.Green
        | 3 -> UnityFS.Simon.SimonColor.Blue
        | _ -> UnityFS.Simon.SimonColor.Yellow

    let randomColorSequence round =
        let rnd = new System.Random()
        [for _ in 1..round -> (intToColor (rnd.Next(1,5)))]

    let colorToSound color duration =
        match color with
        | UnityFS.Simon.SimonColor.Red -> {frequency = 440.0f<Hz>; volume = 30.0f<dB>; duration = duration }
        | UnityFS.Simon.SimonColor.Green -> {frequency = 440.0f<Hz>; volume = 30.0f<dB>; duration = duration }
        | UnityFS.Simon.SimonColor.Blue -> {frequency = 440.0f<Hz>; volume = 30.0f<dB>; duration = duration }
        | UnityFS.Simon.SimonColor.Yellow -> {frequency = 440.0f<Hz>; volume = 30.0f<dB>; duration = duration }
        | _ -> {frequency = 440.0f<Hz>; volume = 30.0f<dB>; duration = duration }

    let FindGameCubeByColor (cubes: UnityFS.Simon.GameCubeBase[]) (c: UnityFS.Simon.SimonColor) =
        let result =
            cubes
            |> Array.filter (fun cube -> cube.CubeColor = c)
        result.[0]
        

    let agentBus<'T> : AgentBus<'T> = Map.empty<string, Agent<'T>>
    let eventBus<'T> : EventBus<'T> = Map.empty<string, Event<'T>>

