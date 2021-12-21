export default { 
    Quadratic: class Quadratic {
        static SolveQuadratic(a, b, c) {
            let vertex = (-b / (2 * a));
            let adjHeight = (c / a);

            adjHeight = (adjHeight - (vertex * vertex)) * -1;
            if(adjHeight < 0) {
                return [NaN, NaN];
            }

            let distance = Math.sqrt(adjHeight);
            return [vertex - distance, vertex + distance];
        }
    }
}