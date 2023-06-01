module Functions

let inline (=>>) func1 func2 x = func2 (func1 x) x

let inline (?=) (q: bool) (yes: 'a, no: 'a) = if q then yes else no

let inline curry f x y = f (x, y)

let inline mapFst f (x, y) = f x, y

let inline mapSnd f (x, y) = x, f y

let inline mapTo (y, z) x =
    (y x, z x)
    
let inline mapBoth (f1, f2) (arg1, arg2) =
    (f1 arg1, f2 arg2)

let inline toTuple (KeyValue(k,v)) = (k,v)
