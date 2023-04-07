namespace AppearCollapser.Database

module Loader =
    let load directory =
        let appears = Appear.load directory
        let tables = Table.loadTables appears directory  
        (appears, tables)