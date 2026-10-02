from typing import List, TypeVar, Optional
import threading

T = TypeVar('T')

MASK64 = 0xFFFFFFFFFFFFFFFF


class SplitMix64:
    def __init__(self, seed: int):
        self._state = (seed + 0x9E3779B97F4A7C15) & MASK64

    def next(self) -> int:
        self._state = (self._state + 0x9E3779B97F4A7C15) & MASK64
        z = self._state
        z = ((z ^ (z >> 30)) * 0xBF58476D1CE4E5B9) & MASK64
        z = ((z ^ (z >> 27)) * 0x94D049BB133111EB) & MASK64
        return (z ^ (z >> 31)) & MASK64


class Xoshiro256PlusPlus:
    def __init__(self, seed: int):
        self.s = [0, 0, 0, 0]
        sm = SplitMix64(seed)
        for i in range(4):
            self.s[i] = sm.next()

    @staticmethod
    def _rotl(x: int, k: int) -> int:
        return ((x << k) | (x >> (64 - k))) & MASK64

    def next(self) -> int:
        result = (self._rotl((self.s[0] + self.s[3]) & MASK64, 23) + self.s[0]) & MASK64
        t = (self.s[1] << 17) & MASK64

        self.s[2] ^= self.s[0]
        self.s[3] ^= self.s[1]
        self.s[1] ^= self.s[2]
        self.s[0] ^= self.s[3]

        self.s[2] ^= t
        self.s[3] = self._rotl(self.s[3], 45)

        return result


class XRandom:
    _thread_local = threading.local()

    def __init__(self, seed: int = 1234567):
        self._rng = Xoshiro256PlusPlus(seed)

    @classmethod
    def get_instance(cls) -> 'XRandom':
        if not hasattr(cls._thread_local, 'instance') or cls._thread_local.instance is None:
            cls.init()
        return cls._thread_local.instance

    @classmethod
    def init(cls, seed: int = 1234567):
        cls._thread_local.instance = XRandom(seed)

    def next(self) -> int:
        return self._rng.next()

    def next_int(self, min_val: int, max_val: int) -> int:
        if min_val > max_val:
            raise ValueError("min > max")
        rng_range = max_val - min_val + 1
        return min_val + int(self._rng.next() % rng_range)

    def shuffle(self, lst: List[T]) -> None:
        for i in range(len(lst) - 1, 0, -1):
            j = self.next_int(0, i)
            lst[i], lst[j] = lst[j], lst[i]
