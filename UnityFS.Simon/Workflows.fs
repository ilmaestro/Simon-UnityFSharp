namespace UnitFS.Simon

module Workflows =
    open UnityFS.Simon
    open UnityFS.Simon.Helpers
    open UnityFS.Simon.Agents
    open UnityEngine

    let gameWorkflow(maxRounds: int, startEvt: Event<GameCubeBase[]>, mouseEvt: Event<SimonColor>, roundUpdater: (int -> unit)) = async {
        let! cubes = Async.AwaitEvent startEvt.Publish
        let agents = agentBus<Message>
        let theBus =
            agents
            |> (cubeHandler cubes)
            |> (buttonListener mouseEvt)
            |> outputHandler
        let output = theBus.TryFind("output").Value
        let input = theBus.TryFind("buttons").Value
            
        let rec gameLoop (state: GameState) = async {
            Debug.Log(state)
                
            let duration =
                match state with
                | Start (r, _) -> 1100.0f<ms> - ((float32 r) * (1000.0f<ms> / (float32 maxRounds)))
                | NextGuess (r, list, g) -> 250.0f<ms>
                | _ -> 1000.0f<ms>
     
            let nextstate =
                match state with
                | NewGame rnds ->
                    //Start a new game, initialize the list of colors!
                    let list = randomColorSequence rnds
                    Debug.Log(list)
                    Start (1, list)
                | Start (r, list) ->
                    //Start a new round, play the computer colors for this round
                    roundUpdater(r)
                    let colors = [for i in 1..r -> list.[i-1]]
                    output.PostAndReply(fun replyChannel -> (ComputerColors (colors, duration, replyChannel)))
                    ComputerPlayed (r, list)
                | ComputerPlayed (r, list) -> 
                    NextGuess (r, list, 0)
                | NextGuess (r, list, g) ->
                    //get player input
                    let choice = input.PostAndReply(fun replyChannel -> (ChooseColor replyChannel))
                    output.PostAndReply(fun replyChannel -> (PlayerColor (choice, duration, replyChannel)))
                    if choice = list.[g] then
                        PlayerGuessed (r, list, (Success g))
                    else PlayerGuessed(r, list, Failure)
                | PlayerGuessed (r,list, gr) ->
                    //handle the result of the players guess
                    match gr with // g is zero-based and r is one-based.. gotta do (r-1) to compare
                    | Success g when g = (r-1) && r = maxRounds -> Win
                    | Success g when g = (r-1) -> NextRound (r, list)
                    | Success g -> NextGuess (r, list, (g+1))
                    | Failure -> Lose
                | NextRound (r, list) -> Start ((r + 1), list)
                | Win -> NewGame maxRounds
                | Lose -> NewGame maxRounds
                | _ -> Invalid

            do! Async.Sleep 10 //minimum time between movements
            return! gameLoop(nextstate)  
            }
        return! gameLoop (NewGame maxRounds)
        }
