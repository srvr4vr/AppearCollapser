module SafeBuilder

type SafeBuilder() =
    member m.Return a = Ok a
    
    member m.Bind(r,fn) =
        match r with
        | Ok(a) -> fn a
        | Error(m) -> Error(m)
        
    member m.TryWith(r,fn) =
      try r() with ex -> fn ex
      
    member m.Delay(f) = f
    
    member m.Run(f) = f()
    
    member m.ReturnFrom(r) = r
        
let safe = SafeBuilder()