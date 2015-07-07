namespace UnityFS.Simon
open UnityEngine
open UnityFS.Simon.Helpers
open UnityFS.Simon.Agents
open UnitFS.Simon.Workflows
open UnityEngine.UI

type GameCube() =
    inherit GameCubeBase()
    
    let mutable isActive = false
    let mutable wasActive = true
    let mutable material = Unchecked.defaultof<Material>
    let mutable audioSource = Unchecked.defaultof<AudioSource>
    
    member this.Start() =
        let rb = this.GetComponent<Rigidbody>()
        material <- this.GetComponent<Renderer>().material
        audioSource <- this.GetComponent<AudioSource>()
        //rb.angularVelocity <- Random.insideUnitSphere * 1.001f
        this.Deactivate()

    member this.Update() =
        match isActive, wasActive with
        | true, false ->
            let cubeMaterialColor = 
                match this.CubeColor with
                | SimonColor.Red -> UnityEngine.Color.red
                | SimonColor.Blue -> UnityEngine.Color.blue
                | SimonColor.Green -> UnityEngine.Color.green
                | _ -> UnityEngine.Color.yellow
            audioSource.Play()
            material.SetColor("_Color", cubeMaterialColor)
            wasActive <- true
        | false, true -> 
            audioSource.Stop()
            material.SetColor("_Color", UnityEngine.Color.black)
            wasActive <- false
        | _,_ -> ()
    
    override this.Deactivate() =
        isActive <- false
        
    override this.Activate() =
        isActive <- true


type GameController() =
    inherit MonoBehaviour()

    [<SerializeField>][<DefaultValue>]
    val mutable MaxRounds: int
    [<SerializeField>][<DefaultValue>]
    val mutable roundText : Text

    let startEvt = Event<GameCubeBase[]>()
    let mouseEvt = Event<SimonColor>()

    member this.Start() =
        let gobjs = GameObject.FindGameObjectsWithTag("GameCube")
        let cubes = gobjs |> Array.map (fun c -> (c.GetComponent<GameCube>()) :> GameCubeBase)
        startEvt.Trigger cubes

    member this.Update() =
        if Input.GetMouseButtonDown(0) then
            let ray = Camera.main.ScreenPointToRay(Input.mousePosition)
            let (result, hit) = Physics.Raycast(ray)
            match (result, hit) with
            | true, hit ->
                let gc = hit.transform.GetComponent<GameCube>()
                mouseEvt.Trigger gc.CubeColor
            | _ -> ()
    member this.UpdateRound(round) =
        this.roundText.text <- "Round: " + round.ToString() + " of " + this.MaxRounds.ToString()

    member this.Awake() =
        gameWorkflow(this.MaxRounds, startEvt, mouseEvt, (fun i -> this.UpdateRound(i))) |> Async.StartImmediate |> ignore
