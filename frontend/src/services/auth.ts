import type { AuthResult, Credentials } from '@/types'

const mockLoginTime = (duration: number) => {
  return new Promise((resolve) => {
    setTimeout(() => {
      resolve(true)
    }, duration)
  })
}

export const login = async (credentials: Credentials): Promise<AuthResult> => {
  await mockLoginTime(2500)

   // TODO
  if (credentials.email === 'lara@local.dev')
  {
    return {
      token: 'i-bims-1-jwt-töken',
      user: {
        id: 1,
        name: 'Lara Test',
        email: credentials.email
      }
    }
  }

  return {
    token: undefined,
    user: undefined
  }
}
