﻿namespace Informedica.GenSolver.Lib

/// A non empty/null string
type name = Name of string
/// A non zero positive rational number
type value = Value of BigRational


/// A range is an unlimited set of
/// rational numbers, when a set has
/// both a minimum, maximum and an 
/// increment then it is not a range
/// anymore but a list of values
type range = 
    | All
    | Incr    of value
    | Min     of value
    | Max     of value
    | MinMax  of value * value
    | IncrMin of value * value

/// Values is a discrete set of 
/// non-zero positive rational numbers,
/// the set is either limited
/// and then it is a list or
/// it is unlimited and then it 
/// is a range.
type values =
    | Values of value list
    | Range of range


/// A Variable has a name 
/// and values
type variable =
    {
        Name: name
        Values: values
    }


module Variable =

    /// Funcions to handle `name`
    module Name =

        /// Create a Name that
        /// is a non empty string
        let create n = n |> Name

    /// Functions to handle `value`
    module Value =
        
        /// Create a Value that 
        /// is a non-zero positive
        /// number
        let create n = n |> Value
    
    /// Functions to handle `values`
    module Values =

        /// Convert `BigRational` list to 
        /// `value` list
        let toValues = List.map Value.create

        /// Create `values` from either a list of
        /// `BigRational` or an incr, min, max combi
        let create vals incr min max =
            if vals |> List.isEmpty |> not then vals |> Values
            else
                match incr, min, max with
                | None,      None,     None     -> All                    |> Range
                | Some incr, None,     None     -> incr        |> Incr    |> Range
                | None,      Some min, None     -> min         |> Min     |> Range
                | None,      None,     Some max -> max         |> Max     |> Range
                | None,      Some min, Some max -> (min, max)  |> MinMax  |> Range
                | Some incr, Some min, None     -> (incr, min) |> IncrMin |> Range

                | Some (Value(incr)), None, Some(Value( max)) -> 
                    [incr..incr..max] |> toValues |> Values
                | Some (Value(incr)), Some(Value(min)), Some(Value( max)) -> 
                    [min..incr..max]  |> toValues |> Values

        /// Aply the give functions to `values`
        /// where fv is used for `value list` and
        /// fr is used for `range`
        let apply fv fr = function
            | Values x -> x |> fv
            | Range x  -> x |> fr

        /// Count the number of values
        /// returns 0 when `values` is
        /// `range`
        let count = 
            let fv = List.length
            let fr = fun _ -> 0
            apply fv fr

    /// Create a variable
    let create n vs = { Name = n; Values = vs }

