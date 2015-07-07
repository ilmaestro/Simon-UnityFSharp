namespace UnityFS.Simon
module Helpers =
    open UnityFS.Simon
    open UnityEngine

    let intToColor int =
        match int with
        | 1 -> SimonColor.Red
        | 2 -> SimonColor.Green
        | 3 -> SimonColor.Blue
        | _ -> SimonColor.Yellow

    let randomColorSequence round =
        let rnd = new System.Random()
        [for _ in 1..round -> (intToColor (rnd.Next(1,5)))]

    let FindGameCubeByColor (cubes: UnityFS.Simon.GameCubeBase[]) (c: SimonColor) =
        let result =
            cubes
            |> Array.filter (fun cube -> cube.CubeColor = c)
        result.[0]

    let agentBus<'T> : AgentBus<'T> = Map.empty<string, Agent<'T>>
