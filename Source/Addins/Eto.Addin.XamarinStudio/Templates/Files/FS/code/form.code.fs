namespace ${Namespace}

open System
open Eto.Forms
open Eto.Drawing

type ${EscapedIdentifier}Base () as this =
    inherit Form ()

    do
        base.Content <- new Label (Text = "Some Content")
