import util from 'util';

class Logger {
    constructor(logLevel=0) {
        this.logLevel = logLevel;
    }
    
    inspect(msg) {
        return typeof msg !== "string" ? util.inspect(msg) : msg
    }
    
    debug(msg) {
        if(this.logLevel >= 2) console.log(`[debug] ${this.inspect(msg)}`)
    }
    
    info(msg) {
        console.log(`[info] ${this.inspect(msg)}`)
    }
    
    error(msg) {
        console.warn(`[error] ${this.inspect(msg)}`)
    }
    
    warn(msg) {
        if(this.logLevel >= 1) console.warn(`[warn] ${this.inspect(msg)}`)
    }
}

export default Logger;