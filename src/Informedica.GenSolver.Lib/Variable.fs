﻿namespace Informedica.GenSolver.Lib


open System

/// Contains functions to handle 
/// the `Variable` type and the types
/// `Variable` depends on:
///
/// * `Name`
/// * `Value`
/// * `Values`
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Variable =


    /// Funcions to handle `Name`
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Name =

        // #region ---- Exceptions ----

        exception NullOrWhiteSpaceException

        // #endregion

        // #region ---- TYPES -----

        /// Represents a non empty/null string identifying a `variable`
        type Name = Name of string

        // #endregion

        // #region ---- CREATE -----

        /// Create with `succ` function when success
        /// and `fail` function when failure
        let createCont succ fail n =
            if n |> String.IsNullOrWhiteSpace then n |> fail
            else n |> Name |> succ

        /// Returns a `Name` option when creation
        /// succeeds
        let createSome = createCont Some (fun _ -> None)

        /// Create a Name that
        /// is a non empty string
        /// Note: this function will fail
        /// when string is null or white space
        let create = createCont id (fun _ -> raise NullOrWhiteSpaceException)

        // #endregion

    /// Functions to handle `Value`
    /// A `Value` is a non zero 
    /// positive value.
    /// Basic arrhythmic operations
    /// can be performed with this type.
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Value =
        
        // #region ---- EXCEPTIONS ----

        exception NonZeroOrPositiveValueException of BigRational

        // #endregion

        // #region ---- TYPES ----

        /// Represents a non zero positive rational number.
        type Value = Value of BigRational

        // #endregion

        // #region ---- APPLY -----

        /// Apply a function `f` to value `x`.
        let apply f (Value x): 'a = f x

        // #endregion

        // #region ----- CREATE ----

        /// Creates a `Value` and calls 
        /// `succ` when success and `fail` when
        /// failure.
        let createCont succ fail n =
            if n <= 0N then n |> fail
            else n |> Value |> succ

        /// Create `Value` option when
        /// success.
        let createSome = createCont Some (fun _ -> None)

        /// Create a Value that 
        /// is a non-zero positive
        /// number.
        let create = 
            let fail n = n |> NonZeroOrPositiveValueException |> raise
            createCont id fail

        /// Zero value
        let zero = 0N |> Value

        /// One value
        let one = 1N |> Value

        /// Two value
        let two = 2N |> Value

        // #endregion

        // #region ---- GETTERS ----

        /// Get the `BigRational` from `value`
        let get = apply id

        // #endregion

        // #region ---- CALCULATION ---- 

        /// Apply an infix operation `op` to
        /// two values `v1` and `v2`
        let calc op (Value v1) (Value v2) =
            v1 |> op <| v2 |> create 

        /// Check whether a value `v` is 
        /// an increment of `incr`.
        let isIncr (Value incr) (Value v) = 
            (v.Numerator * incr.Denominator) % (incr.Numerator * v.Denominator) = 0I

        /// Overload basic arrhythmic operations
        type Value with
            /// Multiplication
            static member (*) (v1, v2) = calc (*) v1 v2
            /// Division</br>
            /// Note that because the type cannot be zero
            /// this operation always succeds.
            static member (/) (v1, v2) = calc (/) v1 v2
            /// Addition
            static member (+) (v1, v2) = calc (+) v1 v2
            /// Subtraction </br>
            /// Note that because the result 
            /// has to be larger than zero, the
            /// operation can fail.
            static member (-) (v1, v2) = calc (-) v1 v2

        // #endregion

    /// Functions to handle `Values`
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Values =

        open System.Collections.Generic
        open Informedica.GenSolver.Utils
        open Value

        // #region Exceptions

        exception MinLargerThanMaxException of BigRational * BigRational

        // #endregion

        // #region ---- TYPES ----

        /// `Values` is a discrete set of 
        /// non-zero positive rational numbers,
        /// the set is either limited
        /// and then it is a list or
        /// it is unlimited and then it 
        /// is a range.
        type Values =
            | ValueSet of Value Set
            | Range of Range
        /// A `Range` is an unlimited set of
        /// rational numbers, when a set has
        /// both a minimum, maximum and an 
        /// increment then it is not a range
        /// anymore but a list of values
        and Range = 
            | All
            | Incr    of Value
            | Min     of Value
            | Max     of Value
            | MinMax  of Value * Value
            | IncrMin of Value * Value

        // #endregion
        
        // #region ---- APPLY -----

        /// Aply the give functions to `Values`
        /// where fv is used for `Value list` and
        /// fr is used for `Range`
        let apply fv fr vs : 'a =
            match vs with
            | ValueSet x -> x |> fv
            | Range x  -> x |> fr

        /// Apply given functions to `Range`.
        let applyRange all fIncr fMin fMax fMinMax fIncrMin r : 'a = 
            match r with 
            | All -> all
            | Incr incr -> incr |> fIncr
            | Min min   -> min  |> fMin
            | Max max   -> max  |> fMax
            | MinMax (min, max) -> fMinMax min max
            | IncrMin (incr, min) -> fIncrMin incr min

        // #endregion

        // #region ---- HELPERS ----

        let optChoose x1 x2 = if x2 |> Option.isSome then x2.Value else x1

        /// Convert `BigRational` list to 
        /// `Value` list. Removes zero and
        /// negative values.
        let bigRtoValueList vs =
            vs
            |> List.filter ((<) 0N)
            |> List.map Value.create

        let valueSetToBigR vs =
            vs 
            |> Set.toList
            |> List.map (fun (Value v) -> v)

        /// Small helper function to turn a sequence
        /// of `Value` to a `Value Set`.
        let seqToValueSet vs = vs |> Set.ofSeq |> ValueSet

        let intersect vs1 vs2 = apply (Set.intersect vs1 >> ValueSet) (fun x -> x |> Range) vs2

        /// Small helper function to turn a `Value Set`
        /// to a `Value list`.
        let valueSetToList = apply Set.toList (fun _ -> [])

        // #endregion

        // #region ---- FILTERS -----

        /// Filter a set of values according
        /// to increment, min and max constraints
        let filter incr min max = 
            let returnTrue = fun _ -> true
            let fIncr = function | None -> returnTrue | Some incr -> Value.isIncr incr
            let fMin  = function | None -> returnTrue | Some min ->  (<=) min
            let fMax  = function | None -> returnTrue | Some max ->  (>=) max
            let fv = Set.filter (fun v -> v |> fIncr incr &&
                                          v |> fMin min &&
                                          v |> fMax max)
                     >> seqToValueSet
            let fr = Range
            apply fv fr

        // #endregion

        // #region ----- CREATE ----

        let rangeAll = Range.All |> Range

        /// Create 'Values' using either the list of
        /// Values, `vals` or using the provided
        /// `incr`, `min` and/or `max`. </br>
        /// * Note * that when both increment, minimum
        /// and maximum or increment and maximum are 
        /// given, a list of values is generated.</br>
        /// * Note * that when both a list is given and
        /// increment and/or minimum and/or maximum, the 
        /// list is filtered.
        let createCont succ fail  incr min max vals =
            // create range all
            let all = All   |> Range |> succ 
            // create range incr, min or max
            let range n = n |> Range |> succ
            // create range MinMax
            let minMax (min, max) =
                if min > max then (min, max)       |> fail
                else (min, max) |> MinMax |> Range |> succ
            // create range IncrMin
            let incrMin (incr, min) = (incr, min) |> IncrMin |> Range |> succ
            // create valueset from incr, max
            let incrMax (incr, max) =
                if incr > max then (incr, max) |> fail
                else 
                    let Value(incr'), Value(max') = incr, max
                    [incr'..incr'..max'] |> bigRtoValueList |> seqToValueSet |> succ
            // create valueset from incr, min, max
            let incrMinMax (incr, min, max) =
                match incr, min, max with
                | _ when min > max -> (min, max)   |> fail
                | _ when incr > max -> (incr, max) |> fail
                | _ -> 
                    let Value(min'), Value(incr'), Value(max') = min, incr, max
                    [min'..incr'..max'] |> bigRtoValueList |> seqToValueSet |> succ

            if vals |> List.isEmpty |> not then 
                vals 
                |> seqToValueSet 
                |> filter incr min max
                |> succ
            else
                match incr, min, max with
                | None,      None,     None     -> all
                | Some incr, None,     None     -> incr |> Incr |> range
                | None,      Some min, None     -> min  |> Min  |> range
                | None,      None,     Some max -> max  |> Max  |> range
                | None,      Some min, Some max -> (min, max)   |> minMax
                | Some incr, Some min, None     -> (incr, min)  |> incrMin
                | Some incr, None,     Some max -> (incr, max)  |> incrMax
                | Some incr, Some min, Some max -> (incr, min, max) |> incrMinMax

        /// Create `Values` from either a list of
        /// `BigRational` or an incr, min, max combi.
        /// *Note: returns `None` when
        /// minimum or increment is larger than maximum.
        let createSome = createCont Some (fun _ -> None)


        /// Create `Values` from either a list of
        /// `BigRational` or an incr, min, max combi.
        /// *Note: raises an `MinLargerThanMaxException`when
        /// minimum or increment is larger than maximum.
        let create = 
            let fail ((Value min), (Value max)) = raise (MinLargerThanMaxException(min, max))
            createCont id fail

        /// Create `Values` directly from a list of 
        /// `BigRational`.
        let createValues = bigRtoValueList >> (create None None None)

        /// Create a `Range` with increment `incr`,
        /// minimum `min` and maximum `max`.</br>
        /// Note that if both increment and maximum 
        /// are given a list of values is created with minimum
        /// of increment and if all arguments values then 
        /// likewise a list of values is generated, i.e. 
        /// `[min..incr..max]`
        let createRange incr min max = create incr min max []

        // #endregion

        // #region ---- GETTERS ----

        /// Count the number of values
        /// returns 0 when `values` is
        /// `Range`.
        let count = 
            let fv = Set.count
            let fr = fun _ -> 0
            apply fv fr

        /// Get the increment from
        /// values if there is one</br>
        /// *Note: a set of values has no increment* 
        let getIncr =
            let none = fun _ -> None
            let fMinMax = fun _ _ -> None
            let fIncrMin = fun incr _ -> Some incr

            let fr = applyRange None Some none none fMinMax fIncrMin
            apply none fr

        /// Get the minimum from
        /// values if there is one
        /// *Note when no minimum is specified 
        /// but there is an increment, then
        /// the increment is the minimum*
        let getMin =
            let none = fun _ -> None
            let fMinMax  = fun min _ -> Some min
            let fIncrMin = fun _ min -> Some min

            let fr = applyRange None Some Some none fMinMax fIncrMin
            apply (fun vs -> vs.MinimumElement |> Some ) fr

        /// Get the maximum from
        /// values if there is one
        let getMax =
            let none = fun _ -> None
            let fMinMax = fun _ max -> max |> Some
            let fIncrMin = fun _ _ -> None

            let fr = applyRange None none none Some fMinMax fIncrMin
            apply (fun vs -> vs.MaximumElement |> Some) fr

        /// Get the values set, returns 
        /// empty set when values is a 
        /// range.
        let getValues = apply id (fun  _ -> Set.empty)

        // #endregion

        // #region ---- SETTERS ----

        let getAll vs = vs |> getIncr, vs |> getMin, vs |> getMax, (vs |> valueSetToList)

        let setIncr incr vs =
            let incr', min, max, vals = vs |> getAll

            if incr' |> Option.isSome then vs
            else
                create (Some incr) min max vals

        let setMin min vs =
            let incr, min', max, vals = vs |> getAll

            match min' with
            | Some min'' -> if min > min'' then create incr (Some min) max vals else vs
            | None       -> create incr (Some min) max vals

        let setMax max vs =
            let incr, min, max', vals = vs |> getAll

            match max' with
            | Some max'' -> if max < max'' then create incr min (Some max) vals else vs
            | None       -> create incr min (Some max) vals

        let setValues vals vs =
            let incr, min, max, vals' = vs |> getAll

            let vals' = vals' |> Set.ofList
            let vals = vals |> seqToValueSet |> filter incr min max //|> valueSetToList
            create incr min max (intersect vals' vals |> valueSetToList)

        /// Set values `v2` to values `v1`. Returns
        /// the intersection of both.
        let setTo v1 v2 = 
            match v1, v2 with
            | ValueSet v1', ValueSet v2' -> v1' |> Set.intersect v2' |> ValueSet
            | Range r, ValueSet v
            | ValueSet v, Range r ->
                let vs                = v |> ValueSet
                let fAll              = vs
                let fIncr incr        = vs |> filter (Some incr) None None
                let fMin min          = vs |> filter None (Some min) None
                let fMax max          = vs |> filter None None (Some max)
                let fMinMax min max   = vs |> filter None (Some min) (Some max)
                let fIncrMin incr min = vs |> filter (Some incr) (Some min) None
                // Filter the values with r
                r |> applyRange fAll fIncr fMin fMax fMinMax fIncrMin

            | Range _, Range _ -> failwith "Not implemented"


        // #endregion

        // #region ---- SOLVE MIN MAX ----

        let solveProductMinMax x1 x2 y =

            let set cmp op getf setf x = function
                | Some v1, Some v2 ->
                    let r = v1 |> op <| v2
                    match x |> getf with
                    | Some v3 -> 
                        if cmp r v3 then x |> setf r 
                        else x 
                        |> Some
                    | None -> x |> setf r |> Some
                | _ -> None

            let setXmin = set (>) (/) getMin setMin
            let setXmax = set (<) (/) getMax setMax

            let setYmin = set (>) (*) getMin setMin 
            let setYmax = set (<) (*) getMax setMax

            // Rule 1: x2.min = y.min / x1.max if x1.max = set && y.max / x1.max > x2.min || x2.min = not set 
            let x1 = setXmin x1 (y |> getMin, x2 |> getMax) |> optChoose x1
            let x2 = setXmin x2 (y |> getMin, x1 |> getMax) |> optChoose x2

            // Rule 2: x2.max = y.max / x1.min if x1.min = set and vice versa
            let x1 = setXmax x1 (y |> getMax, x2 |> getMin) |> optChoose x1
            let x2 = setXmax x2 (y |> getMax, x1 |> getMin) |> optChoose x2
            // Rule 3: y.min = Product(x.min) if all x.min = set
            let y = setYmin y (x1 |> getMin, x2 |> getMin) |> optChoose y
            // Rule 4: y.max = Product(x.max) if all x.max = set
            let y = setYmax y (x2 |> getMax, x2 |> getMax) |> optChoose y
        
            [y;x1;x2]

        let solveSumMinMax vars sum =
        
            let sumVar getf vars  =
                vars 
                |> List.map getf 
                |> List.filter Option.isSome
                |> List.map Option.get
                |> List.fold (+) Value.zero

            let set cmp getf setf v var =
                match v, var |> getf with
                | Some v', Some x -> 
                    if cmp v' x then var |> setf v' |> Some
                    else None
                | Some v', None   -> 
                    if v' > Value.zero then var |> setf v' |> Some
                    else None
                | _ -> None
        
            let setMax = set (<) (getMax) (setMax)
            let setMin = set (>) (getMin) (setMin)
        
            // Rule 1: var.max = sum.max if var.max = not set || sum.max < var.max
            let vars = vars |> List.map (fun v ->
                v |> setMax (sum |> getMax) |> optChoose v)

            // Rule 2: var.min = sum.min - Sum(var.min) if n - 1 var.min = set
            let vars = 
                match vars |> List.filter(getMin >> Option.isNone) with
                | [var] -> 
                    let min = 
                        match sum |> getMin with
                        | None -> None
                        | Some min -> min - (vars 
                                             |> List.map getMin 
                                             |> List.filter Option.isSome 
                                             |> List.map Option.get
                                             |> List.fold (+) Value.zero) 
                                      |> Some
                    let var' = var |> setMin min |> optChoose var
                    vars |> List.replace ((=) var) var'
                | _ -> vars

            // Rule 3: sum.min = Sum(var.min) if Sum(var.min) > sum.min
            let sum =
                setMin (match sumVar getMin vars with | v when v > Value.zero -> v |> Some | _ -> None) sum
                |> optChoose sum
            
            // Rule 4: sum.max = Sum(var.max) if all var.max = set
            let sum =
                if vars |> List.map getMax |> List.exists Option.isNone then sum
                else
                    setMax (match sumVar getMax vars with | v when v > Value.zero -> v |> Some | _ -> None) sum |> optChoose sum
        
            sum::vars

            // #endregion
        
        // #region ---- CALCULATION -----

        /// Applies an infix operator
        /// to two `Values`. Only add values
        /// to the result set if > 0.
        let calc op = function
            | ValueSet s1, ValueSet s2 ->
                let s1 = new ResizeArray<_>(s1)
                let s2 = new ResizeArray<_>(s2)
                let s3 = new ResizeArray<_>()
                // Check whether the operand is subtraction
                let opIsSubtr = (Value.two |> op <| Value.one) = Value.one

                for x1 in s1 do
                    for x2 in s2 do
                        // prevent subtraction resulting in a zero or negative result
                        if opIsSubtr && x1 > x2 || (not opIsSubtr) then 
                            // perform the arrhythmic operation and to the result set
                            s3.Add(x1 |> op <| x2) 
                new HashSet<_>(s3, HashIdentity.Structural) |> seqToValueSet
            // Do not perform any calcuation when one of the args is not
            // a list of values
            | _ -> Range.All |> Range

        /// Function to determine how one range
        /// constraints another range.
        let constrainRangeWith incr min max = failwith "Not implemented yet"
            

        // Extend type with basic arrhythmic operations.
        type Values with
            /// Multiply 
            static member (*) (vs1, vs2) = calc (*) (vs1, vs2)
            /// Divide
            static member (/) (vs1, vs2) = calc (/) (vs1, vs2)
            /// Add
            static member (+) (vs1, vs2) = calc (+) (vs1, vs2)
            /// Subtract
            static member (-) (vs1, vs2) = calc (-) (vs1, vs2)
            /// Add `expr` to `res`
            static member (=!) (res, expr) = expr |> setTo res

        // #endregion


    module Dto =
        
        [<AllowNullLiteralAttribute>]
        type Dto () =
            member val Name = null : string with get, set
            member val Values = [||] : BigRational[] with get, set
            member val Increment = None : BigRational option with get, set
            member val Minimum = None:  BigRational option with get, set
            member val Maximum = None: BigRational option with get, set

        let create n = new Dto(Name = n)

    
    // #region ---- TYPES ----

    /// Represents a variable in an
    /// `Equation`. The variable is 
    /// identified by `Name` and has
    /// a set of possible `Values`.
    type Variable =
        {
            Name: Name.Name
            Values: Values.Values
        }

    // #endregion

    // #region ---- CREATE -----

    /// Create a variable
    let create n vs = { Name = n; Values = vs }

    ///  Create a variable from `Variable.Dto.Dto`.
    let fromDto (dto: Dto.Dto) =
        let createValue = Option.bind Value.createSome
        
        let name = dto.Name |> Name.create
        let incr = dto.Increment |> createValue
        let min  = dto.Minimum   |> createValue
        let max  = dto.Maximum   |> createValue
        let vs = Values.bigRtoValueList (dto.Values |> Array.toList)
        let vs = Values.create incr min max vs

        create name vs

    let toDto (v: Variable) =

        let someValueToBigR = function
            | Some v' -> let (Value.Value v) = v' in v |> Some
            | None    -> None

        let dto = Dto.create (let (Name.Name n) = v.Name in n)

        dto.Increment <- v.Values |> Values.getIncr |> someValueToBigR
        dto.Minimum   <- v.Values |> Values.getMin  |> someValueToBigR
        dto.Maximum   <- v.Values |> Values.getMax  |> someValueToBigR

        dto.Values    <- v.Values |> Values.getValues |> Values.valueSetToBigR |> List.toArray

        dto

    // #endregion

