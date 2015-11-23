﻿// Learn more about F# at http://fsharp.net. See the 'F# Tutorial' project
// for more guidance on F# programming.
#I "../../src/Informedica.GenSolver.Lib/"
#I "Scripts/"
#load "load-references.fsx"
#load "Variable.fs"

// Define your library scripting code here
open Informedica.GenSolver.Lib

open Swensen.Unquote
open FsCheck
open NUnit.Framework
        


// ToDo add this to tests
let checkAdd l1 l2 =
    let l1 = l1 |> List.filter ((<) 0)
    let l2 = l2 |> List.filter ((<) 0)
    let create = (List.map BigRational.FromInt) >> Variable.Values.createValues
    let add =
        [ for x1 in l1 do
            for x2 in l2 do
                yield x1 + x2 ]
        |> List.toSeq
        // List will only contain distinct values
        |> Seq.distinct
        |> Seq.toList
    let l1' = l1 |> create
    let l2' = l2 |> create
    (l1' + l2') |> Variable.Values.valueSetToList |> List.length = add.Length

let checkSubtr l1 l2 =
    let l1 = l1 |> List.filter ((<) 0)
    let l2 = l2 |> List.filter ((<) 0)
    let create = (List.map BigRational.FromInt) >> Variable.Values.createValues
    let add =
        [ for x1 in l1 do
            for x2 in l2 do
                yield x1 - x2 ]
        |> List.toSeq
        // List will contain only non zero/negative valuees
        |> Seq.filter ((<) 0)
        // List will only contain distinct values
        |> Seq.distinct
        |> Seq.toList
    let l1' = l1 |> create
    let l2' = l2 |> create
    (l1' - l2') |> Variable.Values.valueSetToList |> List.length = add.Length


Check.Quick checkAdd
Check.Quick checkSubtr


module Testing =

    module Variable =

        module Value =

            open Variable.Value
            open Variable.Values

            [<TestFixture>]
            type ``The create function`` () =
                [<Test>]
                member x.``Will not create negative values`` () =
                    let nonZeroOrNegative x =
                        try
                            x 
                            |> BigRational.FromInt
                            |> Variable.Value.create > Variable.Value.zero 
                        with 
                        | Variable.Value.NonZeroOrPositiveValueException _ -> true

                    Check.Quick nonZeroOrNegative
                    
        
            [<TestFixture>]
            type ``Given a zero or negative number`` () =
                [<Test>]
                member x.``With 0 an exception is raised`` () =
                    raises<Variable.Value.NonZeroOrPositiveValueException> <@ 0N |> Variable.Value.create <> (0N |> Value.Value) @>
                [<Test>]
                member x.``With -1N an exception is raised`` () =
                    raises<Variable.Value.NonZeroOrPositiveValueException> <@ 0N |> Variable.Value.create <> (-1N |> Value.Value) @>

            [<TestFixture>]
            type ``Given a non zero positive value`` () =
                [<Test>]
                member x.``A value can be created`` () =
                    test<@ Variable.Value.create 1N = (1N |> Value.Value) @>
        
            [<TestFixture>]
            type ``Given a get function`` () =
                [<Test>]
                member x.``The same value is returned as created`` () =
                    let canGetValue v =
                        if v > 0 then
                            v
                            |> BigRational.FromInt
                            |> Variable.Value.create
                            |> Variable.Value.get
                            |> BigRational.ToInt32 = v
                        else true

                    Check.Quick canGetValue
            
            [<TestFixture>]
            type ``Given an infix operand`` () =
            
                [<Test>]
                member x.``The operand gives the same result as applied to BigRationals`` () =
                    let v1 = 1N |> Variable.Value.create
                    let v2 = 1N |> Variable.Value.create
                    test <@ Variable.Value.calc (+) v1 v2 |> Variable.Value.get = 2N @>
                    test <@ (v1 + v2) |> Variable.Value.get = 2N @>

            [<TestFixture>]
            type ``Is overloaded with`` () =
                [<Test>]
                member x.``Basic arrhythmic functions`` () =
                    let checkMult x1 x2 =
                        let create = BigRational.FromInt >> Variable.Value.create
                        let get = Variable.Value.get >> BigRational.ToInt32
                        if x1 > 0 && x2 > 0 then
                            let x1' = x1 |> create
                            let x2' = x2 |> create
                            (x1' + x2') |> get = (x1 + x2)
                        else true

                    let checkDiv x1 x2 =
                        let create = BigRational.FromInt >> Variable.Value.create
                        let get = Variable.Value.get >> BigRational.ToInt32
                        if x1 > 0 && x2 > 0 then
                            let x1' = x1 |> create
                            let x2' = x2 |> create
                            (x1' + x2') |> get = (x1 + x2)
                        else true

                    let checkAdd x1 x2 =
                        let create = BigRational.FromInt >> Variable.Value.create
                        let get = Variable.Value.get >> BigRational.ToInt32
                        if x1 > 0 && x2 > 0 then
                            let x1' = x1 |> create
                            let x2' = x2 |> create
                            (x1' + x2') |> get = (x1 + x2)
                        else true

                    let checkSubtr x1 x2 =
                        let create = BigRational.FromInt >> Variable.Value.create
                        let get = Variable.Value.get >> BigRational.ToInt32
                        if x1 > 0 && x2 > 0 then
                            let x1' = x1 |> create
                            let x2' = x2 |> create
                            (x1' + x2') |> get = (x1 + x2)
                        else true

                    Check.Quick checkMult
                    Check.Quick checkDiv
                    Check.Quick checkAdd
                    Check.Quick checkSubtr
        
            [<TestFixture>]
            type ``Given a negative subtraction result`` () =
                
                [<Test>]
                member x.``A NonZeroOrPositive error is thrown`` () =
                    let v1 = Variable.Value.create 1N
                    let v2 = Variable.Value.create 2N
                    raises<Variable.Value.NonZeroOrPositiveValueException> <@ v1 - v2 @> 

        module Values =

            open  Variable.Values

            [<TestFixture>]
            type ``Given list = empty incr = None min = None max = None`` () =
                let incr = None
                let min = None
                let max = None

                let vals = Range.All |> Values.Range
        
                [<Test>]
                member x.``Creating values returns range All`` () =
                    test <@ Variable.Values.create incr min max [] = vals @>
        
                [<Test>]
                member x.``Counting values returns zero`` () =
                    test <@ Variable.Values.create incr min max [] |> Variable.Values.count = 0 @>

            [<TestFixture>]
            type ``Given list with one value incr = None min = None max = None`` () =
                let incr = None
                let min = None
                let max = None
                // List with one value
                let vals = [1N |> Variable.Value.create] 

                [<Test>]
                member x.``Counting values returns one`` () =
                    test <@ Variable.Values.create incr min max vals |> Variable.Values.count = 1 @>
        
                [<Test>]
                member x.``Creating values returns list with one value`` () =
                    test <@ Variable.Values.create incr min max vals = (vals |> Variable.Values.seqToValueSet) @>

            [<TestFixture>]
            type ``Given a list of Value`` () =
    
                [<Test>]
                member x.``The resulting ValueSet contains an equal amount`` () =
                    let equalCount c =
                        if c >= 1 then
                            let vals = 
                                [1..c] 
                                |> List.map BigRational.FromInt
                                |> Variable.Values.createValues
                            vals |> Variable.Values.valueSetToList |> List.length = c
                        else true

                    Check.Quick equalCount

                [<Test>]
                member x.``Can filter by incr, min and max`` () =
                    // Check values filter
                    let create = Variable.Values.createValues
                    let vsincr = [1N..1N..10N] |> create
                    let incr = Variable.Value.create 2N |> Some
                    let min  = Variable.Value.create 4N |> Some 
                    let max  = Variable.Value.create 8N |> Some
                    test <@ Variable.Values.filter None None None vsincr = vsincr @>
                    test <@ Variable.Values.filter incr None None vsincr = ([2N..2N..10N] |> create) @>
                    test <@ Variable.Values.filter incr min None vsincr  = ([6N..2N..10N] |> create) @>
                    test <@ Variable.Values.filter incr min max vsincr   = ([6N..2N..6N] |> create) @>
                    

let runTests () =

    let test = new Testing.Variable.Value.``Given a zero or negative number``()
    test.``With 0 an exception is raised``()
    test.``With -1N an exception is raised``()

    let test = new Testing.Variable.Value.``Given a negative subtraction result``()
    test.``A NonZeroOrPositive error is thrown``()
    
    let test = new Testing.Variable.Value.``Given a non zero positive value``()
    test.``A value can be created``()

    let test = new Testing.Variable.Value.``The create function``()
    test.``Will not create negative values``()

    let test = new Testing.Variable.Value.``Given a get function``()
    test.``The same value is returned as created``()

    let test = new Testing.Variable.Value.``Given an infix operand``()
    test.``The operand gives the same result as applied to BigRationals``()

    let test = new Testing.Variable.Value.``Is overloaded with``()
    test.``Basic arrhythmic functions``()

    let test = new Testing.Variable.Values.``Given list = empty incr = None min = None max = None``()
    test.``Counting values returns zero``()
    test.``Creating values returns range All``()

    let test = new Testing.Variable.Values.``Given list with one value incr = None min = None max = None``()
    test.``Counting values returns one``()
    test.``Creating values returns list with one value``()
    
    let test = new Testing.Variable.Values.``Given a list of Value``()
    test.``The resulting ValueSet contains an equal amount``()
    test.``Can filter by incr, min and max``()

