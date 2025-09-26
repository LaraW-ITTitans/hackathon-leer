interface Credentials {
  email: string
  password: string
  remember: boolean
}

type User = {
  id: string
  name: string
  email: string
}

interface AuthResult {
  token: string
  user: User
}

type UserState = {
  isAuthenticated: boolean
  user: User | undefined
  accessToken: string | undefined
};

export type {
  Credentials,
  User,
  AuthResult,
  UserState,
};
