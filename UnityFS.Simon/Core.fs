namespace UnityFS.Simon

open UnityEngine

//=========================
// Game Domain
//=========================

type Agent<'T> = MailboxProcessor<'T>
type AgentBus<'T> = Map<string, Agent<'T>>
type EventBus<'T> = Map<string, Event<'T>>

[<Measure>] type Hz
[<Measure>] type dB
[<Measure>] type ms

type Note = {frequency: float32<Hz>; volume: float32<dB>; duration: float32<ms>}
type SimonColor =
| Red = 0
| Blue = 1
| Green = 2
| Yellow = 3

type Button =
| On of SimonColor
| Off

type Round = int
type Guess = int
type GuessResult =
| Success of Guess
| Failure

type RoundState =
    {TotalRounds: int; Round: int; }

type GameState =
| NewGame of Round
| Start of Round * SimonColor list
| PlayerGuessed of Round * SimonColor list * GuessResult
| NextGuess of Round * SimonColor list * Guess
| ComputerPlayed of Round * SimonColor list
| NextRound of Round * SimonColor list
| Win
| Lose
| Invalid

type Message =
| ChooseColor of AsyncReplyChannel<SimonColor>
| PlayerColor of SimonColor * float32<ms> * AsyncReplyChannel<unit>
| ComputerColor of SimonColor * float32<ms> * AsyncReplyChannel<unit>
| ActivateColor of SimonColor * AsyncReplyChannel<unit>
| DeactivateColor of SimonColor * AsyncReplyChannel<unit>
    
type IGameCube =
    abstract member Activate : unit -> unit
    abstract member Deactivate : unit -> unit

type GameCubeBase() =
    inherit MonoBehaviour()

    [<SerializeField>][<DefaultValue>]
    val mutable CubeColor: SimonColor

    abstract member Activate: unit -> unit
    abstract member Deactivate: unit -> unit

    default this.Activate() = ()
    default this.Deactivate() = ()