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
type Color =
| Red = 0
| Blue = 1
| Green = 2
| Yellow = 3

type Button =
| On of Color
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
| Start of Round * Color list
| PlayerGuessed of Round * Color list * GuessResult
| NextGuess of Round * Color list * Guess
| ComputerPlayed of Round * Color list
| NextRound of Round * Color list
| Win
| Lose
| Invalid

type Message =
| ChooseColor of AsyncReplyChannel<Color>
| PlayerColor of Color * float32<ms> * AsyncReplyChannel<unit>
| ComputerColor of Color * float32<ms> * AsyncReplyChannel<unit>
| ActivateColor of Color * AsyncReplyChannel<unit>
| DeactivateColor of Color * AsyncReplyChannel<unit>
    
type IGameCube =
    abstract member Activate : unit -> unit
    abstract member Deactivate : unit -> unit

type GameCubeBase() =
    inherit MonoBehaviour()

    [<SerializeField>][<DefaultValue>]
    val mutable CubeColor: Color

    abstract member Activate: unit -> unit
    abstract member Deactivate: unit -> unit

    default this.Activate() = ()
    default this.Deactivate() = ()