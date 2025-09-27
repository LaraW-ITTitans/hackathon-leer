import { jwtDecode } from 'jwt-decode'
import type { AuthResult, Credentials, User } from '@/types'
import { authApi, userApi } from '@/api'
import { LoginBindingModel } from '@/api/codegen'

const mockLoginTime = (duration: number) => {
  return new Promise((resolve) => {
    setTimeout(() => {
      resolve(true)
    }, duration)
  })
}

export const login = async (credentials: Credentials): Promise<AuthResult> => {
  const loginResponse = await authApi.login(new LoginBindingModel({
    userName: credentials.email,
    password: credentials.password,
    rememberMe: credentials.remember,
  }))

  // ok
  if (loginResponse && loginResponse.token)
  {
    const decodedToken = jwtDecode(loginResponse.token)

    const user = await userApi.getCurrentUser()
    await mockLoginTime(750)

    return {
      token: loginResponse.token,
      expiration: decodedToken.exp,
      user: {
        id: decodedToken['Hackathon-User-ID'],
        name: user.userName,
        displayName: decodedToken['Hackathon-Display-Name'],
        claims: decodedToken['Hackathon-Auth'],
        skills: user.skills,
      } as User
    } as AuthResult
  }

  return {
    token: undefined,
    user: undefined
  }
}
