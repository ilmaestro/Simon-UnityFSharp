namespace UnityFS.Simon
open UnityEngine
open UnityFS.Simon.Helpers
open UnityFS.Simon.Agents

type GameCube() =
    inherit GameCubeBase()

    [<DefaultValue>]
    val mutable renderer : Renderer

    [<DefaultValue>]
    val mutable isActive : bool

    member this.Start() =
        let rb = this.GetComponent<Rigidbody>()
        this.renderer <- this.GetComponent<Renderer>()
        rb.angularVelocity <- Random.insideUnitSphere * 1.001f
        this.Deactivate()

    member this.Update() =
        match this.isActive with
        | true ->
            let cubeMaterialColor = 
                match this.CubeColor with
                | UnityFS.Simon.Color.Red -> UnityEngine.Color.red
                | UnityFS.Simon.Color.Blue -> UnityEngine.Color.blue
                | UnityFS.Simon.Color.Green -> UnityEngine.Color.green
                | _ -> UnityEngine.Color.yellow
            this.renderer.material.SetColor("_Color", cubeMaterialColor)
        | false -> 
            this.renderer.material.SetColor("_Color", UnityEngine.Color.black)
    
    override this.Deactivate() =
        this.isActive <- false
        
    override this.Activate() =
        this.isActive <- true


type GameController() =
    inherit MonoBehaviour()

    [<SerializeField>][<DefaultValue>]
    val mutable MaxRounds: int

    let startEvt = Event<GameCubeBase[]>()
    let updateEvt = Event<float32>()
    let mouseEvt = Event<UnityFS.Simon.Color>()

    member this.Start() =
        let gobjs = GameObject.FindGameObjectsWithTag("GameCube")
        let cubes = gobjs |> Array.map (fun c -> (c.GetComponent<GameCube>()) :> GameCubeBase)
        Debug.Log("Got cubes...")
        Debug.Log(cubes.Length)
        startEvt.Trigger cubes
        ()

    member this.Update() =
        updateEvt.Trigger Time.deltaTime
        if Input.GetMouseButtonDown(0) then
            let ray = Camera.main.ScreenPointToRay(Input.mousePosition)
            let (result, hit) = Physics.Raycast(ray)
            match (result, hit) with
            | true, hit ->
                let gc = hit.transform.GetComponent<GameCube>()
                mouseEvt.Trigger gc.CubeColor
            | _ -> ()
        ()

    member this.Awake() =
        let gameWorkflow = async {
            let! cubes = Async.AwaitEvent startEvt.Publish
            let events = eventBus<UnityFS.Simon.Color>.Add("inputs", Event<UnityFS.Simon.Color>())
            let agents = agentBus<Message>
            let theBus =
                agents
                |> piezoEmitter
                |> (ledEmitter cubes)
                |> (buttonListener mouseEvt)
                |> outputHandler
            let output = theBus.TryFind("output").Value
            let input = theBus.TryFind("buttons").Value
            
            let rec gameLoop (state: GameState) = async {
                Debug.Log(state)
                
                let duration =
                    match state with
                    | Start (r, _) -> 1000.0f<ms>
                    | NextGuess (r, list, g) -> 1000.0f<ms> - ((float32 r) * 100.0f<ms>)
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
                        for i in 1..r do
                            let c = list.Item (i-1)
                            output.PostAndReply(fun replyChannel -> (ComputerColor (c, duration, replyChannel)))
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
                        | Success g when g = (r-1) && r = this.MaxRounds -> Win
                        | Success g when g = (r-1) -> NextRound (r, list)
                        | Success g -> NextGuess (r, list, (g+1))
                        | Failure -> Lose
                    | NextRound (r, list) -> Start ((r + 1), list)
                    | Win -> NewGame this.MaxRounds
                    | Lose -> NewGame this.MaxRounds
                    | _ -> Invalid

                do! Async.Sleep 100 //minimum time between movements
                return! gameLoop(nextstate)  
                }
            return! gameLoop (NewGame this.MaxRounds)
            }
        gameWorkflow
        |> Async.StartImmediate
        |> ignore
        ()
