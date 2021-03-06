﻿#load "load-references-release.fsx"
#load "load-project-release.fsx"

#time


open Informedica.GenSolver.Lib

module VAR = Variable
module N = VAR.Name
module VR = VAR.ValueRange
module E = Equation

let varCount v = VAR.count

let varIsSolved v = VAR.isSolved

let varIsSolvable =  VAR.isSolvable

let solve e = e |> E.solve [e]

let isSolved = E.isSolved

let isSolvable = E.isSolvable

let createVar n vs min incr max = 
    let min' = min |> Option.bind ((VR.createMin false) >> Some)
    let max' = max |> Option.bind ((VR.createMax false) >> Some)

    let vs' =
        vs 
        |> Set.ofList

    let vr =
        match min, incr, max with
        | None, None, None when vs |> List.isEmpty -> VR.unrestricted
        | _ ->  VR.createExc false vs' min' incr max'

    VAR.createSucc (n |> N.createExc) vr

let y = createVar "y" [2N; 4N] None None None
let x1 = createVar "x1" [] None None None
let x2 = createVar "x2" [1N] None None None

E.createProductEqExc (y, []) |> solve
E.createSumEqExc (y, [])     |> solve

E.createProductEqExc (y, [x1;x2]) |> solve //|> snd |> isSolvable
E.createSumEqExc (y, [x1;x2])     |> solve //|> snd |> isSolvable

E.createProductEqExc (y, [x1;x2])

