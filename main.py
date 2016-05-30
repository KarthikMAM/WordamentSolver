__author__ = "Karthik M A M"


class WordamentSolver:
    """
        README.txt

        Definition of Object variables
        a = the matrix array containing the wordament board
        s = the stack containing the active game configuration of available moves - FURTUE MOVES
        r = the stack containing the game configuration interms of the current sequence - PAST MOVES
        o = the set containging the membership of the previous moves to do conflict resolution

        This algorithm uses the following concepts
            1. Goal stack planning
            2. Neighbor hood search techniques

    """

    
    #Initialization of the Solver
    def __init__(self, seq):
        filler = '0'
        n_cols = 5

        self.a = [ tuple( filler * (n_cols + 2)) ] + [ tuple((filler + seq[i:i + n_cols] + "0").ljust(n_cols + 2, '0')) for i in range(0, len(seq), n_cols) ] + [ tuple( '0' * (n_cols + 2) ) ]
        self.s = [ ]
        self.r = [ ]
        self.o = [ ]


        self.cache = { }

    #Finds the next character
    def findNext(self, q):
        s = self.s
        a = self.a
        r = self.r
        
        x, y, l = s.pop()

        o = set()
        while len(r) > 0 and r[-1][2] == l:
            o |= { r.pop()[:2] }
        self.o -= o
    
        found = -1
        for i in [x - 1, x, x + 1]:
            for j in [y - 1, y, y + 1]:
                if (i, j) != (x, y) and (i, j) not in self.o and a[i][j] == q:  #conflict resolution using membership set o
                    s.append( ( i, j, l + 1) )
                    found = 1
    
        if found == 1:
            r.append( (x, y, l) )
            self.o |= { (x, y) }
                
        return found                
    
    #Finds the string q which may start at (x,y)
    def findStringAt(self, x, y, q):
        if q[0] == self.a[x][y]:
            self.s, self.r, self.o = [ (x, y, 0) ], [ ], set()
            while len(self.s) > 0:

                self.debug()
        
                if self.s[-1][2] == len(q) - 1:
                    return [ (self.a[x][y], x, y, c) for x, y, c in self.r + [ self.s[-1] ] ]
                
                self.findNext(q[self.s[-1][2] + 1])
        else:
            return False

    #Iterate through the matrix to find the string q
    def findString(self, q):
        for i in range(1, len(self.a) - 1):
            for j in range(1, len(self.a[0]) - 1):
                 res = self.findStringAt(i, j, q)
                 if bool(res):
                     self.cache[q] = res
                     return res
        else:
            self.cache[q] = "{0} is not found".format(q)
            return self.cache[q]

    #Display matrix
    def dispMat(self):
        print(*self.a, sep='\n', end='\n\n')
        print("################################################################################")

    #Debug the stack of the PAST + PRESENT states
    def debug(self):
        for i, j, l in self.r + [ ( 0, 0, 20 ) ] + self.s:
            print( '--' * (l + 1), (self.a[i][j], i, j))
        print("################################################################################")
        
    #Find the list of the strings requested by the user
    def findStrings(self, q):
        print("################################################################################")
        self.dispMat()
        print("################################################################################")
        for i in q:
            print("################################################################################")
            print(self.cache.get(i , self.findString(i)))
            print("################################################################################")
            print('\n\n')

if __name__ == "__main__":
    s, *q = input().split()
    WordamentSolver(s).findStrings(q)
