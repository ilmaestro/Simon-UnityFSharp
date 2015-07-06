namespace UnityFS.Simon
module Helpers =
    open UnityFS.Simon

    let intToColor int =
        match int with
        | 1 -> UnityFS.Simon.Color.Red
        | 2 -> UnityFS.Simon.Color.Green
        | 3 -> UnityFS.Simon.Color.Blue
        | _ -> UnityFS.Simon.Color.Yellow

    let randomColorSequence round =
        let rnd = new System.Random()
        [for _ in 1..round -> (intToColor (rnd.Next(1,5)))]

    let colorToSound color duration =
        match color with
        | UnityFS.Simon.Color.Red -> {frequency = 440.0f<Hz>; volume = 30.0f<dB>; duration = duration }
        | UnityFS.Simon.Color.Green -> {frequency = 440.0f<Hz>; volume = 30.0f<dB>; duration = duration }
        | UnityFS.Simon.Color.Blue -> {frequency = 440.0f<Hz>; volume = 30.0f<dB>; duration = duration }
        | UnityFS.Simon.Color.Yellow -> {frequency = 440.0f<Hz>; volume = 30.0f<dB>; duration = duration }
        | _ -> {frequency = 440.0f<Hz>; volume = 30.0f<dB>; duration = duration }

    let FindGameCubeByColor (cubes: UnityFS.Simon.GameCubeBase[]) (c: UnityFS.Simon.Color) =
        let result =
            cubes
            |> Array.filter (fun cube -> cube.CubeColor = c)
        result.[0]
        

    let agentBus<'T> : AgentBus<'T> = Map.empty<string, Agent<'T>>
    let eventBus<'T> : EventBus<'T> = Map.empty<string, Event<'T>>